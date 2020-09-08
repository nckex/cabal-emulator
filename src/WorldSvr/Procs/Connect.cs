using HPT.Logging;
using Microsoft.Extensions.Logging;
using Share.System;
using Share.Encryption;
using Share.Serde;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static Share.Protodef;
using static Share.Protos.Connect;
using static Share.Protosdef;
using static WorldSvr.Protos.Connect;
using static WorldSvr.Protos.WorldServer;
using static Share.Protos.Context;

namespace WorldSvr.Procs
{
    class Connect
    {
        public static async Task OnC2S_CONNECT2SERV(WorldSvrSession session, ArraySegment<byte> buffer)
        {
            const ushort STEP = 0x37E5;

            CabalEncryption.Instance.ChangeStep(session.Encryptable, STEP);

            var s2cConnect2Serv = new S2C_CONNECT2SERV()
            {
                Recv2ndXorSeed = CabalEncryption.KEY,
                AuthKey = session.SessionTime,
                Index = session.SessionId,
                RecvXorKeyIdx = STEP
            };

            using var s2cConnect2ServS = await CabalSerializer.Instance.SerializeAsync(s2cConnect2Serv, Opcode.CSC_CONNECT2SERV);
            await session.SendAsync(s2cConnect2ServS);
        }

        public static async Task OnC2S_VERIFYLINKS(WorldSvrSession session, ArraySegment<byte> buffer)
        {
            var c2sVerifyLinks = await CabalSerializer.Instance.DeserializeAsync<C2S_VERIFYLINKS>(buffer);

            var ipsLink = new IPS_LINK()
            {
                FromService = new IPS_SERVICE()
                {
                    GroupIdx = Config.Instance.WorldSvr.GroupIdx,
                    ServerIdx = Config.Instance.WorldSvr.ServerIdx
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
                    UserNum = session.CharUserContext.UserNum,
                    AuthKey = session.CharUserContext.AuthKey,
                    UseACSUB = session.CharUserContext.UseACSUB,
                    UseWHSUB = session.CharUserContext.UseWHSUB,
                    UseEQSUB = session.CharUserContext.UseEQSUB,
                    IsWHLOCK = session.CharUserContext.IsWHLOCK,
                    IsEQLOCK = session.CharUserContext.IsEQLOCK,
                    ServiceType = session.CharUserContext.ServiceType,
                    ServiceKind = session.CharUserContext.ServiceKind,
                    ExpirationDate = session.CharUserContext.ExpirationDate,
                    ExtendedCharCreation = session.CharUserContext.ExtendedCharCreation
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

            if (WorldSvrServer.Instance.TryGetSession(ipsLink.ToSession.SessionId, ipsLink.ToSession.SessionTime, out var session))
            {
                session.InitAccountContext();

                session.CharUserContext.UserNum = ipsLink.UserContextData.UserNum;
                session.CharUserContext.AuthKey = ipsLink.UserContextData.AuthKey;
                session.CharUserContext.UseACSUB = ipsLink.UserContextData.UseACSUB;
                session.CharUserContext.UseWHSUB = ipsLink.UserContextData.UseWHSUB;
                session.CharUserContext.UseEQSUB = ipsLink.UserContextData.UseEQSUB;
                session.CharUserContext.IsWHLOCK = ipsLink.UserContextData.IsWHLOCK;
                session.CharUserContext.IsEQLOCK = ipsLink.UserContextData.IsEQLOCK;
                session.CharUserContext.ServiceType = ipsLink.UserContextData.ServiceType;
                session.CharUserContext.ServiceKind = ipsLink.UserContextData.ServiceKind;
                session.CharUserContext.ExpirationDate = ipsLink.UserContextData.ExpirationDate;
                session.CharUserContext.ExtendedCharCreation = ipsLink.UserContextData.ExtendedCharCreation;

                ipsResultLink.IsLinked = true;
            } else
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

            if (!WorldSvrServer.Instance.TryGetSession(ipsResultLink.ToSession.SessionId, ipsResultLink.ToSession.SessionTime, out var session))
            {
                CustomLogger<Connect>.Instance
                    .LogError("OnIPC_RESULT_LINK(): Link failed, could not find ToSession({0}:{1})",
                        ipsResultLink.ToSession.SessionId,
                        ipsResultLink.ToSession.SessionTime);

                return;
            }

            if (ipsResultLink.IsLinked)
            {
                session.CharUserContext.ShouldSoftDisconnect = true;
            }

            var s2cVerifyLinks = new S2C_VERIFYLINKS()
            {
                ServerIdx = ipsResultLink.FromService.ServerIdx,
                GroupIdx = ipsResultLink.FromService.GroupIdx,
                IsLinked = ipsResultLink.IsLinked
            };

            using var ipsVerifyLinksS = await CabalSerializer.Instance.SerializeAsync(s2cVerifyLinks, Opcode.CSC_VERIFYLINKS);
            await session.SendAsync(ipsVerifyLinksS);

            session.Disconnect();
        }

        public static async Task OnIPC_FDISCONNECT(ArraySegment<byte> buffer)
        {
            var ipsFDisconnect = await IPCSerializer.Instance.DeserializeAsync<IPS_FDISCONNECT>(buffer);

            var isSessionFound = WorldSvrServer.Instance
                .TryGetSession(ipsFDisconnect.ToSession.SessionId, ipsFDisconnect.ToSession.SessionTime, out var session);

            var ipsResultFDisconnect = new IPS_RESULT_FDISCONNECT()
            {
                FromService = new IPS_SERVICE()
                {
                    GroupIdx = Config.Instance.WorldSvr.GroupIdx,
                    ServerIdx = Config.Instance.WorldSvr.ServerIdx
                },
                UserNum = ipsFDisconnect.UserNum
            };

            if (isSessionFound && session.CharUserContext.UserNum == ipsFDisconnect.UserNum)
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
