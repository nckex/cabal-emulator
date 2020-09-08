using Dapper;
using GlobalMgrSvr.Resources;
using HPT.Logging;
using Microsoft.Extensions.Logging;
using Share.Serde;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using static Share.Protos.CharLobby;

namespace GlobalMgrSvr.Procs
{
    class CharLobby
    {
        public static async Task OnIPC_CHECKPASSWD(GlobalMgrSession session, ArraySegment<byte> buffer)
        {
            var ipsCheckPasswd = await IPCSerializer.Instance.DeserializeAsync<IPS_CHECKPASSWD>(buffer);

            using var conn = new SqlConnection(Config.Instance.GlobalMgr.ConnectionString);
            var spCabalCheckPasswd = await conn.QueryFirstOrDefaultAsync<SP_CABAL_CHECKPASSWD>("sp_cabal_checkpasswd", new
            {
                pUserNum = ipsCheckPasswd.UserNum,
                pPassword = ipsCheckPasswd.Password
            }, commandType: CommandType.StoredProcedure);

            if (spCabalCheckPasswd == null)
            {
                CustomLogger<CharLobby>.Instance
                    .LogError("OnIPC_CHECKPASSWD(): Failed to check password! UserNum({0})",
                    ipsCheckPasswd.UserNum);

                return;
            }

            var ipsResultCheckPasswd = new IPS_RESULT_CHECKPASSWD()
            {
                ToSession = ipsCheckPasswd.FromSession,
                Checked = spCabalCheckPasswd.Checked
            };

            using var ipsResultCheckPasswdS = await IPCSerializer.Instance.SerializeAsync(ipsResultCheckPasswd, Share.Opcode.IPC_RESULT_CHECKPASSWD);
            await session.SendAsync(ipsResultCheckPasswdS);
        }
    }
}
