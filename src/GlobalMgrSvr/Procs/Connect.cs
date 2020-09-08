using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using GlobalMgrSvr.System;
using HPT.Logging;
using Microsoft.Extensions.Logging;
using Share.Serde;
using static Share.Protodef;
using static Share.Protos.Connect;

namespace GlobalMgrSvr.Procs
{
    class Connect
    {
        public static async Task OnIPC_LINK(GlobalMgrSession _, ArraySegment<byte> buffer)
        {
            var ipsLink = await IPCSerializer.Instance.DeserializeAsync<IPS_LINK>(buffer);

            if (!ServiceStateHandler.Instance.TryGet(ipsLink.ToService.GroupIdx, ipsLink.ToService.ServerIdx, out var serviceContext))
            {
                CustomLogger<Connect>.Instance
                    .LogError("OnIPC_LINK(): Link request fail! (GroupIdx {0} ServerIdx {1}) -> (GroupIdx {2} ServerIdx {3})",
                    ipsLink.FromService.GroupIdx,
                    ipsLink.FromService.ServerIdx,
                    ipsLink.ToService.GroupIdx,
                    ipsLink.ToService.ServerIdx);
                
                return;
            }

            if (AccountLoginStateHandler.Instance.IsStateActiveOnService(ipsLink.UserContextData.UserNum, ipsLink.ToService.GroupIdx, ipsLink.ToService.ServerIdx))
            {
                CustomLogger<Connect>.Instance
                    .LogError("OnIPC_LINK(): Link is duplicated! (GroupIdx {0} ServerIdx {1}) -> (GroupIdx {2} ServerIdx {3})",
                    ipsLink.FromService.GroupIdx,
                    ipsLink.FromService.ServerIdx,
                    ipsLink.ToService.GroupIdx,
                    ipsLink.ToService.ServerIdx);

                return;
            }

            await serviceContext.Session.SendAsync(buffer);
        }

        public static async Task OnIPC_RESULT_LINK(GlobalMgrSession _, ArraySegment<byte> buffer)
        {
            var ipsResultLink = await IPCSerializer.Instance.DeserializeAsync<IPS_RESULT_LINK>(buffer);

            if (!ServiceStateHandler.Instance.TryGet(ipsResultLink.ToService.GroupIdx, ipsResultLink.ToService.ServerIdx, out var serviceContext))
            {
                CustomLogger<Connect>.Instance
                    .LogError("OnIPC_RESULT_LINK(): Link result fail! (GroupIdx {0} ServerIdx {1}) -> (GroupIdx {2} ServerIdx {3})",
                    ipsResultLink.FromService.GroupIdx,
                    ipsResultLink.FromService.ServerIdx,
                    ipsResultLink.ToService.GroupIdx,
                    ipsResultLink.ToService.ServerIdx);

                return;
            }

            if (ipsResultLink.IsLinked)
            {
                var isStateAdded = AccountLoginStateHandler.Instance
                    .TryAddState(
                        ipsResultLink.UserNum,
                        ipsResultLink.FromService.GroupIdx,
                        ipsResultLink.FromService.ServerIdx,
                        ipsResultLink.FromSession.SessionId,
                        ipsResultLink.FromSession.SessionTime);


                if (!isStateAdded)
                {
                    CustomLogger<Connect>.Instance
                        .LogError($"OnIPC_RESULT_LINK(): Failed to set AccountState! UserNum({ipsResultLink.UserNum})");

                    return;
                }
            }

            await serviceContext.Session.SendAsync(buffer);
        }

        public static async Task OnIPC_RESULT_FDISCONNECT(GlobalMgrSession _, ArraySegment<byte> buffer)
        {
            var ipsResultFDisconnect = await IPCSerializer.Instance.DeserializeAsync<IPS_RESULT_FDISCONNECT>(buffer);

            if (!ipsResultFDisconnect.SessionFound)
            {
                AccountLoginStateHandler.Instance
                    .TryRemoveState(ipsResultFDisconnect.UserNum, ipsResultFDisconnect.FromService.GroupIdx, ipsResultFDisconnect.FromService.ServerIdx);

                using var conn = new SqlConnection(Config.Instance.GlobalMgr.ConnectionString);
                await conn.ExecuteAsync("sp_cabal_loginstate", new
                {
                    pUserNum = ipsResultFDisconnect.UserNum,
                    pMode = LoginStateMode.SoftDisconnect
                }, commandType: CommandType.StoredProcedure);
            }
        }
    }
}
