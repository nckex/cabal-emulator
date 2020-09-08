using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using HPT.Logging;
using LoginSvr.System;
using Share.Serde;
using Share.System;
using static Share.Protodef;
using static Share.Protosdef;
using static Share.Protos.Auth;
using static LoginSvr.Protos.Auth;

namespace LoginSvr.Procs
{
    class Auth
    {
        public static async Task OnC2S_PRE_AUTHENTICATE(LoginSvrSession session, ArraySegment<byte> _)
        {
            using var s2cPreAuthenticate = await CabalSerializer.Instance.SerializeAsync(new S2C_PRE_AUTHENTICATE(), Opcode.CSC_PRE_AUTHENTICATE);
            await session.SendAsync(s2cPreAuthenticate);
        }

        public static async Task OnC2S_RSA_PUBLIC_KEY(LoginSvrSession session, ArraySegment<byte> buffer)
        {
            session.InitRSA();

            var s2cRsaPublicKey = new S2C_RSA_PUBLIC_KEY()
            {
                Status = true,
                KeyLength = (ushort)session.RSA.PublicKey.Count,
                PublicKey = session.RSA.PublicKey.Array
            };

            using var s2cRsaPublicKeyS = await CabalSerializer.Instance.SerializeAsync(s2cRsaPublicKey, Opcode.CSC_RSA_PUBLIC_KEY);
            await session.SendAsync(s2cRsaPublicKeyS);
        }

        public static async Task OnC2S_PREAUTH_STATUS(LoginSvrSession session, ArraySegment<byte> _)
        {
            var nfsU2005 = new NFS_U2005()
            {
                U0 = Config.Instance.LoginSvr.LoginTimer
            };

            using var nfsU2005S = await CabalSerializer.Instance.SerializeAsync(nfsU2005, Opcode.NFY_U2005);
            using var s2cPreAuthStatus = await CabalSerializer.Instance.SerializeAsync(new S2C_PREAUTH_STATUS(), Opcode.CSC_PREAUTH_STATUS);

            await session.SendAsync(s2cPreAuthStatus);
            await session.SendAsync(nfsU2005S);
        }

        public static async Task OnC2S_AUTHENTICATE(LoginSvrSession session, ArraySegment<byte> buffer)
        {
            var c2sAuthenticate = await CabalSerializer.Instance.DeserializeAsync<C2S_AUTHENTICATE>(buffer);

            var decryptedData = session.RSA.Decrypt(c2sAuthenticate.EncryptedData);

            var username = Encoding.ASCII.GetString(decryptedData, 0, 129); // 129 - ID string len
            var password = Encoding.ASCII.GetString(decryptedData, 129, 33); // 33 - Password string len

            var ipsAuth = new IPS_AUTH()
            {
                FromService = new IPS_SERVICE()
                {
                    ServerIdx = Config.Instance.LoginSvr.ServerIdx,
                    GroupIdx = Protodef.LOGINSVR_GROUPIDX
                },
                FromSession = new IPS_SESSION()
                {
                    SessionId = session.SessionId,
                    SessionTime = session.SessionTime
                },
                Username = username,
                Password = password,
                IpAddress = session.IPAddress.ToString()
            };

            using var ipsAuthS = await IPCSerializer.Instance.SerializeAsync(ipsAuth, Share.Opcode.IPC_AUTH);
            await GlobalMgrClient.Instance.SendAsync(ipsAuthS);
        }

        public static async Task OnIPC_RESULT_AUTH(ArraySegment<byte> buffer)
        {
            var ipsResultAuth = await IPCSerializer.Instance.DeserializeAsync<IPS_RESULT_AUTH>(buffer);

            if (!LoginSvrServer.Instance.TryGetSession(ipsResultAuth.ToSession.SessionId, ipsResultAuth.ToSession.SessionTime, out var session))
                return;

            var s2cAuthenticate = new S2C_AUTHENTICATE()
            {
                AuthenticateHeader = new S2C_AUTHENTICATE_HEADER()
                {
                    KeepAlive = ipsResultAuth.AuthResult == AuthResult.Success || ipsResultAuth.AuthResult == AuthResult.Already,
                    AuthResult = ipsResultAuth.AuthResult
                }
            };

            using var s2cAuthenticateS = await CabalSerializer.Instance.SerializeAsync(s2cAuthenticate, Opcode.CSC_AUTHENTICATE);
            await session.SendAsync(s2cAuthenticateS);

            if (ipsResultAuth.AuthResult == AuthResult.Already)
            {
                session.UpdateDupUserNum(ipsResultAuth.UserContextData.UserNum);

                var nfsLoginTimer = new NFS_LOGINTIMER()
                {
                    Milliseconds = Config.Instance.LoginSvr.LoginTimer
                };

                using var nfsLoginTimerS = await CabalSerializer.Instance.SerializeAsync(nfsLoginTimer, Opcode.NFY_LOGINTIMER);
                await session.SendAsync(nfsLoginTimerS);

                s2cAuthenticate.AuthenticateHeader.SubMessageType = 13;
                s2cAuthenticate.AuthenticateHeader.KeepAlive = false;
                s2cAuthenticate.AuthenticateHeader.AuthResult = ipsResultAuth.AuthResult;

                using var s2cAuthenticateS2 = await CabalSerializer.Instance.SerializeAsync(s2cAuthenticate, Opcode.CSC_AUTHENTICATE);
                await session.SendAsync(s2cAuthenticateS2);
            }
            else if (ipsResultAuth.AuthResult == AuthResult.Success)
            {
                session.InitAccountContext();

                session.UserContext.UserNum = ipsResultAuth.UserContextData.UserNum;
                session.UserContext.AuthKey = ipsResultAuth.UserContextData.AuthKey;
                session.UserContext.UseACSUB = ipsResultAuth.UserContextData.UseACSUB;
                session.UserContext.UseWHSUB = ipsResultAuth.UserContextData.UseWHSUB;
                session.UserContext.UseEQSUB = ipsResultAuth.UserContextData.UseEQSUB;
                session.UserContext.IsWHLOCK = ipsResultAuth.UserContextData.IsWHLOCK;
                session.UserContext.IsEQLOCK = ipsResultAuth.UserContextData.IsEQLOCK;
                session.UserContext.ServiceType = ipsResultAuth.UserContextData.ServiceType;
                session.UserContext.ServiceKind = ipsResultAuth.UserContextData.ServiceKind;
                session.UserContext.ExpirationDate = ipsResultAuth.UserContextData.ExpirationDate;
                session.UserContext.ExtendedCharCreation = ipsResultAuth.UserContextData.ExtendedCharCreation;

                var ipsLoginState = new IPS_LOGINSTATE()
                {
                    FromService = ipsResultAuth.ToService,
                    FromSession = ipsResultAuth.ToSession,
                    UserNum = session.UserContext.UserNum,
                    Mode = LoginStateMode.Connect
                };

                using var ipsLoginStateS = await IPCSerializer.Instance.SerializeAsync(ipsLoginState, Share.Opcode.IPC_LOGINSTATE);
                await GlobalMgrClient.Instance.SendAsync(ipsLoginStateS);
            }
        }

        public static async Task OnIPC_RESULT_LOGINSTATE(ArraySegment<byte> buffer)
        {
            var ipsResultLoginState = await IPCSerializer.Instance.DeserializeAsync<IPS_RESULT_LOGINSTATE>(buffer);

            if (!LoginSvrServer.Instance.TryGetSession(ipsResultLoginState.ToSession.SessionId, ipsResultLoginState.ToSession.SessionTime, out var session))
                return;

            var nfsLoginTimer = new NFS_LOGINTIMER()
            {
                Milliseconds = Config.Instance.LoginSvr.LoginTimer
            };

            using var nfsLoginTimerS = await CabalSerializer.Instance.SerializeAsync(nfsLoginTimer, Opcode.NFY_LOGINTIMER);

            var nfsSystemMessg = new NFS_SYSTEMMESSG()
            {
                SystemMessage = SystemMessage.NewNormal
            };

            using var nfsSystemMessgS = await CabalSerializer.Instance.SerializeAsync(nfsSystemMessg, Opcode.NFY_SYSTEMMESSG);

            var nfsUrlToClient = new NFS_URLTOCLIENT()
            {
                URL1Len = Config.Instance.LoginSvr.URL1.Length,
                URL1 = Config.Instance.LoginSvr.URL1,
                URL2Len = Config.Instance.LoginSvr.URL2.Length,
                URL2 = Config.Instance.LoginSvr.URL2,
                URL3Len = Config.Instance.LoginSvr.URL3.Length,
                URL3 = Config.Instance.LoginSvr.URL3,
                URL4Len = Config.Instance.LoginSvr.URL4.Length,
                URL4 = Config.Instance.LoginSvr.URL4,
                URL5Len = Config.Instance.LoginSvr.URL5.Length,
                URL5 = Config.Instance.LoginSvr.URL5
            };

            using var nfsUrlToClientS = await CabalSerializer.Instance.SerializeAsync(nfsUrlToClient, Opcode.NFY_URLTOCLIENT);

            var s2cAuthenticate = new S2C_AUTHENTICATE()
            {
                AuthenticateHeader = new S2C_AUTHENTICATE_HEADER()
                {
                    KeepAlive = true,
                    SubMessageType = 13,
                    AuthResult = AuthResult.Success
                },
                UserNum = session.UserContext.UserNum
            };

            using var s2cAuthenticateS = await CabalSerializer.Instance.SerializeAsync(s2cAuthenticate, Opcode.CSC_AUTHENTICATE);

            await session.SendAsync(nfsLoginTimerS);
            await session.SendAsync(nfsUrlToClientS);
            await session.SendAsync(s2cAuthenticateS);
            await session.SendAsync(nfsSystemMessgS);

            ServiceStateHandler.Instance.Subscribe(session.SessionId);
            await ServiceStateHandler.Instance.SendServerStateToSessionAsync(session);
        }

        public static async Task OnC2S_FDISCONNECT(LoginSvrSession session, ArraySegment<byte> _)
        {
            var ipsReqFDisconnect = new IPS_REQ_FDISCONNECT()
            {
                FromSession = new IPS_SESSION()
                {
                    SessionId = session.SessionId,
                    SessionTime = session.SessionTime
                },
                UserNum = session.DupUserNum
            };

            using var ipsReqFDisconnectS = await IPCSerializer.Instance.SerializeAsync(ipsReqFDisconnect, Share.Opcode.IPC_REQ_FDISCONNECT);
            await GlobalMgrClient.Instance.SendAsync(ipsReqFDisconnectS);
        }

        public static async Task OnIPC_RESULT_REQ_FDISCONNECT(ArraySegment<byte> buffer)
        {
            var ipsResultReqFDisconnect = await IPCSerializer.Instance.DeserializeAsync<IPS_RESULT_REQ_FDISCONNECT>(buffer);

            if (!LoginSvrServer.Instance.TryGetSession(ipsResultReqFDisconnect.ToSession.SessionId, ipsResultReqFDisconnect.ToSession.SessionTime, out var session))
                return;

            var s2cFDisconnect = new S2C_FDISCONNECT()
            {
                Success = ipsResultReqFDisconnect.Success
            };

            using var s2cFDisconnectS = await CabalSerializer.Instance.SerializeAsync(s2cFDisconnect, Opcode.CSC_FDISCONNECT);
            await session.SendAsync(s2cFDisconnectS);
        }
    }
}
