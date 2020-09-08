using System;
using System.Threading.Tasks;
using HPT.Logging;
using LoginSvr.System;
using Microsoft.Extensions.Logging;
using Share.System;
using Share.Encryption;
using Share.Serde;
using static LoginSvr.Protos.Auth;
using static LoginSvr.Protos.Connect;
using static Share.Protodef;
using static Share.Protos.Connect;
using static Share.Protosdef;
using static Share.Protos.Context;

namespace LoginSvr.Procs
{
    class Connect
    {
        public static async Task OnC2S_CONNECT2SERV(LoginSvrSession session, ArraySegment<byte> _)
        {
            // var c2sConnect2serv = await CabalSerializer.Instance.DeserializeAsync<C2S_CONNECT2SERV>(buffer);

            const ushort STEP = 0x1FFC;
            CabalEncryption.Instance.ChangeStep(session.Encryptable, STEP);

            var s2cConnect2serv = new S2C_CONNECT2SERV()
            {
                Recv2ndXorSeed = CabalEncryption.KEY,
                AuthKey = session.SessionTime,
                Index = session.SessionId,
                RecvXorKeyIdx = STEP
            };

            using var s2cConnect2servS = await CabalSerializer.Instance.SerializeAsync(s2cConnect2serv, Opcode.CSC_CONNECT2SERV);
            await session.SendAsync(s2cConnect2servS);
        }

        public static async Task OnC2S_CHECKVERSION(LoginSvrSession session, ArraySegment<byte> buffer)
        {
            var c2sCheckVersion = await CabalSerializer.Instance.DeserializeAsync<C2S_CHECKVERSION>(buffer);

            var s2cCheckVersion = new S2C_CHECKVERSION()
            {
                ServerVersion = Config.Instance.LoginSvr.IgnoreClientVersion ? c2sCheckVersion.Version : Config.Instance.LoginSvr.ClientVersion
            };

            using var s2cCheckVersionS = await CabalSerializer.Instance.SerializeAsync(s2cCheckVersion, Opcode.CSC_CHECKVERSION);
            await session.SendAsync(s2cCheckVersionS);
        }

        public static async Task OnC2S_VERIFYLINKS(LoginSvrSession session, ArraySegment<byte> buffer)
        {
            var c2sVerifyLinks = await CabalSerializer.Instance.DeserializeAsync<C2S_VERIFYLINKS>(buffer);

            if (!Config.Instance.LoginSvr.IgnoreClientVersion && (c2sVerifyLinks.NormalClientMagicKey != Config.Instance.LoginSvr.ClientMagicKey))
            {
                CustomLogger<Connect>.Instance
                    .LogWarning("OnC2S_VERIFYLINKS(): Bad MagicKey! Session({0}) ClientMagicKey({1}) ServerMagicKey({2})",
                        session.Signature,
                        c2sVerifyLinks.NormalClientMagicKey,
                        Config.Instance.LoginSvr.ClientMagicKey);

                using var s2cVerifyLinksS = await CabalSerializer.Instance.SerializeAsync(new S2C_VERIFYLINKS(), Opcode.CSC_VERIFYLINKS);
                await session.SendAsync(s2cVerifyLinksS);
                return;
            }

            var ipsLink = new IPS_LINK()
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
                ToService = new IPS_SERVICE()
                {
                    GroupIdx = c2sVerifyLinks.GroupIdx,
                    ServerIdx = c2sVerifyLinks.ServerIdx
                },
                ToSession = new IPS_SESSION()
                {
                    SessionId = c2sVerifyLinks.SessionId,
                    SessionTime = c2sVerifyLinks.SessionTime
                },
                UserContextData = new IPS_USERCONTEXT_DATA()
                {
                    UserNum = session.UserContext.UserNum,
                    AuthKey = session.UserContext.AuthKey,
                    UseACSUB = session.UserContext.UseACSUB,
                    UseWHSUB = session.UserContext.UseWHSUB,
                    UseEQSUB = session.UserContext.UseEQSUB,
                    IsWHLOCK = session.UserContext.IsWHLOCK,
                    IsEQLOCK = session.UserContext.IsEQLOCK,
                    ServiceType = session.UserContext.ServiceType,
                    ServiceKind = session.UserContext.ServiceKind,
                    ExpirationDate = session.UserContext.ExpirationDate,
                    ExtendedCharCreation = session.UserContext.ExtendedCharCreation
                }
            };

            using var ipsLinkS = await IPCSerializer.Instance.SerializeAsync(ipsLink, Share.Opcode.IPC_LINK);
            await GlobalMgrClient.Instance.SendAsync(ipsLinkS);
        }

        public static async Task OnIPC_LINK(ArraySegment<byte> buffer)
        {
            var ipsLink = await IPCSerializer.Instance.DeserializeAsync<IPS_LINK>(buffer);

            var ipsResultLink = new IPS_RESULT_LINK()
            {
                FromService = ipsLink.ToService,
                FromSession = ipsLink.ToSession,
                ToService = ipsLink.FromService,
                ToSession = ipsLink.FromSession,
                UserNum = ipsLink.UserContextData.UserNum
            };

            if (LoginSvrServer.Instance.TryGetSession(ipsLink.ToSession.SessionId, ipsLink.ToSession.SessionTime, out var session))
            {
                session.InitAccountContext();

                session.UserContext.UserNum = ipsLink.UserContextData.UserNum;
                session.UserContext.AuthKey = ipsLink.UserContextData.AuthKey;
                session.UserContext.UseACSUB = ipsLink.UserContextData.UseACSUB;
                session.UserContext.UseWHSUB = ipsLink.UserContextData.UseWHSUB;
                session.UserContext.UseEQSUB = ipsLink.UserContextData.UseEQSUB;
                session.UserContext.IsWHLOCK = ipsLink.UserContextData.IsWHLOCK;
                session.UserContext.IsEQLOCK = ipsLink.UserContextData.IsEQLOCK;
                session.UserContext.ServiceType = ipsLink.UserContextData.ServiceType;
                session.UserContext.ServiceKind = ipsLink.UserContextData.ServiceKind;
                session.UserContext.ExpirationDate = ipsLink.UserContextData.ExpirationDate;
                session.UserContext.ExtendedCharCreation = ipsLink.UserContextData.ExtendedCharCreation;

                ipsResultLink.IsLinked = true;

                await ServiceStateHandler.Instance.SendServerStateToSessionAsync(session);

                var nfsLoginTimer = new NFS_LOGINTIMER()
                {
                    Milliseconds = Config.Instance.LoginSvr.LoginTimer
                };

                using var nfsLoginTimerS = await CabalSerializer.Instance.SerializeAsync(nfsLoginTimer, Opcode.NFY_LOGINTIMER);
                await session.SendAsync(nfsLoginTimerS);
            }
            else
            {
                CustomLogger<Connect>.Instance
                    .LogError("OnIPC_LINK(): Link fail! FromService({0}:{1}) FromSession({2}:{3}) ToService({4}:{5}) ToSession({6}:{7})",
                    ipsLink.FromService.GroupIdx,
                    ipsLink.FromService.ServerIdx,
                    ipsLink.FromSession.SessionId,
                    ipsLink.FromSession.SessionTime,
                    ipsLink.ToService.GroupIdx,
                    ipsLink.ToService.ServerIdx,
                    ipsLink.ToSession.SessionId,
                    ipsLink.ToSession.SessionTime);
            }

            using var ipsResultLinkS = await IPCSerializer.Instance.SerializeAsync(ipsResultLink, Share.Opcode.IPC_RESULT_LINK);
            await GlobalMgrClient.Instance.SendAsync(ipsResultLinkS);
        }

        public static async Task OnIPC_RESULT_LINK(ArraySegment<byte> buffer)
        {
            var ipsResultLink = await IPCSerializer.Instance.DeserializeAsync<IPS_RESULT_LINK>(buffer);

            if (!LoginSvrServer.Instance.TryGetSession(ipsResultLink.ToSession.SessionId, ipsResultLink.ToSession.SessionTime, out var session))
            {
                CustomLogger<Connect>.Instance
                    .LogError("OnIPC_RESULT_LINK(): Link failed, could not find ToSession({0}:{1})",
                        ipsResultLink.ToSession.SessionId,
                        ipsResultLink.ToSession.SessionTime);

                return;
            }

            if (ipsResultLink.IsLinked)
            {
                session.UserContext.ShouldSoftDisconnect = true;
            }

            var s2cVerifyLinks = new S2C_VERIFYLINKS()
            {
                ServerIdx = ipsResultLink.FromService.ServerIdx,
                GroupIdx = ipsResultLink.FromService.GroupIdx,
                IsLinked = ipsResultLink.IsLinked
            };

            using var ipsVerifyLinksS = await CabalSerializer.Instance.SerializeAsync(s2cVerifyLinks, Opcode.CSC_VERIFYLINKS);
            await session.SendAsync(ipsVerifyLinksS);
        }

        public static async Task OnIPC_FDISCONNECT(ArraySegment<byte> buffer)
        {
            var ipsFDisconnect = await IPCSerializer.Instance.DeserializeAsync<IPS_FDISCONNECT>(buffer);

            var isSessionFound = LoginSvrServer.Instance
                .TryGetSession(ipsFDisconnect.ToSession.SessionId, ipsFDisconnect.ToSession.SessionTime, out var session);

            var ipsResultFDisconnect = new IPS_RESULT_FDISCONNECT()
            {
                FromService = new IPS_SERVICE()
                {
                    GroupIdx = Protodef.LOGINSVR_GROUPIDX,
                    ServerIdx = Config.Instance.LoginSvr.ServerIdx
                },
                UserNum = ipsFDisconnect.UserNum
            };

            if (isSessionFound && session.UserContext.UserNum == ipsFDisconnect.UserNum)
            {
                var nfsSystemMessg = new NFS_SYSTEMMESSG()
                {
                    SystemMessage = SystemMessage.ForceDisconnect
                };

                using var nfsSystemMessgS = await CabalSerializer.Instance.SerializeAsync(nfsSystemMessg, Opcode.NFY_SYSTEMMESSG);
                await session.SendAsync(nfsSystemMessgS);

                ipsResultFDisconnect.SessionFound = true;

                session.Disconnect();
            }

            using var ipsResultFDisconnectS = await IPCSerializer.Instance.SerializeAsync(ipsResultFDisconnect, Share.Opcode.IPC_RESULT_FDISCONNECT);
            await GlobalMgrClient.Instance.SendAsync(ipsResultFDisconnectS);
        }
    }
}
