using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using GlobalMgrSvr.Resources;
using GlobalMgrSvr.System;
using HPT.Logging;
using Microsoft.Extensions.Logging;
using Share.Serde;
using static Share.Protodef;
using static Share.Protos.Subpassword;

namespace GlobalMgrSvr.Procs
{
    class Subpassword
    {
        public static async Task OnIPC_SUBPASSWD_ASK(GlobalMgrSession session, ArraySegment<byte> buffer)
        {
            var ipsSubpasswdAsk = await IPCSerializer.Instance.DeserializeAsync<IPS_SUBPASSWD_ASK>(buffer);

            var isSubpasswdRequired = SubpasswordHandler.Instance
                    .IsSubpasswdRequiredFor(ipsSubpasswdAsk.UserNum, SubpasswordType.Account, ipsSubpasswdAsk.IpAddress);

            var ipsResultSubpasswdAsk = new IPS_RESULT_SUBPASSWD_ASK()
            {
                ToSession = ipsSubpasswdAsk.FromSession,
                IsRequired = isSubpasswdRequired
            };

            using var ipsResultSubpasswdAskS = await IPCSerializer.Instance.SerializeAsync(ipsResultSubpasswdAsk, Share.Opcode.IPC_RESULT_SUBPASSWD_ASK);
            await session.SendAsync(ipsResultSubpasswdAskS);
        }

        public static async Task OnIPC_SUBPASSWD_SET(GlobalMgrSession session, ArraySegment<byte> buffer)
        {
            var ipsSubpasswdSet = await IPCSerializer.Instance.DeserializeAsync<IPS_SUBPASSWD_SET>(buffer);

            using var conn = new SqlConnection(Config.Instance.GlobalMgr.ConnectionString);
            var spCabalSubpasswdSet = await conn.QueryFirstOrDefaultAsync<SP_CABAL_SUBPASSWD_SET>("sp_cabal_subpasswd_set", new
            {
                pUserNum = ipsSubpasswdSet.UserNum,
                pMode = ipsSubpasswdSet.Mode,
                pType = ipsSubpasswdSet.Type,
                pPassword = ipsSubpasswdSet.Password.Trim(char.MinValue),
                pQuestion = ipsSubpasswdSet.Question,
                pAnswer = ipsSubpasswdSet.Answer.Trim(char.MinValue).ToLower()
            }, commandType: CommandType.StoredProcedure);

            if (spCabalSubpasswdSet == null)
            {
                CustomLogger<Subpassword>.Instance
                    .LogError("OnIPC_SUBPASSWD_SET(): Failed to set subpassword! UserNum({0}) Mode({}) Type({})",
                    ipsSubpasswdSet.UserNum,
                    ipsSubpasswdSet.Mode,
                    ipsSubpasswdSet.Type);

                return;
            }

            var ipsResultSubpasswdSet = new IPS_RESULT_SUBPASSWD_SET()
            {
                ToSession = ipsSubpasswdSet.FromSession,
                Success = spCabalSubpasswdSet.Success,
                Type = ipsSubpasswdSet.Type,
                Mode = ipsSubpasswdSet.Mode
            };

            using var ipsResultSubpasswdSetS = await IPCSerializer.Instance.SerializeAsync(ipsResultSubpasswdSet, Share.Opcode.IPC_RESULT_SUBPASSWD_SET);
            await session.SendAsync(ipsResultSubpasswdSetS);
        }

        public static async Task OnIPC_SUBPASSWD_AUTH_REMEMBER(GlobalMgrSession session, ArraySegment<byte> buffer)
        {
            var ipsSubpasswdAuthRemember = await IPCSerializer.Instance.DeserializeAsync<IPS_SUBPASSWD_AUTH_REMEMBER>(buffer);

            SubpasswordHandler.Instance
                .CreateSubpasswordDataForUserNum(ipsSubpasswdAuthRemember.UserNum, ipsSubpasswdAuthRemember.Type);

            using var conn = new SqlConnection(Config.Instance.GlobalMgr.ConnectionString);
            var spCabalSubpasswdAuth = await conn.QueryFirstOrDefaultAsync<SP_CABAL_SUBPASSWD_AUTH>("sp_cabal_subpasswd_auth", new
            {
                pUserNum = ipsSubpasswdAuthRemember.UserNum,
                pType = ipsSubpasswdAuthRemember.Type,
                pPassword = ipsSubpasswdAuthRemember.Password.Trim(char.MinValue)
            }, commandType: CommandType.StoredProcedure);

            if (spCabalSubpasswdAuth == null)
            {
                CustomLogger<Subpassword>.Instance
                    .LogError("OnIPC_SUBPASSWD_AUTH_REMEMBER(): Failed to auth remember subpassword! UserNum({0}) Type({1})",
                    ipsSubpasswdAuthRemember.UserNum,
                    ipsSubpasswdAuthRemember.Type);

                return;
            }

            var ipsResultSubpasswdAuth = new IPS_RESULT_SUBPASSWD_AUTH()
            {
                ToSession = ipsSubpasswdAuthRemember.FromSession,
                ToOpcode = ipsSubpasswdAuthRemember.FromOpcode,
                Success = spCabalSubpasswdAuth.Success,
                Type = ipsSubpasswdAuthRemember.Type
            };

            if (spCabalSubpasswdAuth.Success)
            {
                SubpasswordHandler.Instance
                    .ResetPassFailtCount(ipsSubpasswdAuthRemember.UserNum, ipsSubpasswdAuthRemember.Type);

                SubpasswordHandler.Instance
                    .UpdateSubpasswdData(
                        ipsSubpasswdAuthRemember.UserNum,
                        ipsSubpasswdAuthRemember.Type,
                        ipsSubpasswdAuthRemember.IPAddress32, 
                        ipsSubpasswdAuthRemember.RememberHours);
            }
            else
            {
                var failCount = SubpasswordHandler.Instance
                    .IncrementAndReturnPassFailCount(
                        ipsSubpasswdAuthRemember.UserNum,
                        ipsSubpasswdAuthRemember.Type);

                ipsResultSubpasswdAuth.TryNum = failCount;

                if (Config.Instance.GlobalMgr.SubpwdFailBlock && failCount >= Config.Instance.GlobalMgr.SubpwdFailLimit)
                {
                    SubpasswordHandler.Instance
                        .RemoveSubpasswdData(ipsSubpasswdAuthRemember.UserNum);

                    await conn.ExecuteAsync("sp_cabal_subpasswd_block", new
                    {
                        pUserNum = ipsSubpasswdAuthRemember.UserNum
                    }, commandType: CommandType.StoredProcedure);

                    ipsResultSubpasswdAuth.AccountBlocked = true;
                }
            }

            using var ipsResultSubpasswdAuthS = await IPCSerializer.Instance.SerializeAsync(ipsResultSubpasswdAuth, Share.Opcode.IPC_RESULT_SUBPASSWD_AUTH_REMEMBER);
            await session.SendAsync(ipsResultSubpasswdAuthS);
        }

        public static async Task OnIPC_SUBPASSWD_AUTH(GlobalMgrSession session, ArraySegment<byte> buffer)
        {
            var ipsSubpassdAuth = await IPCSerializer.Instance.DeserializeAsync<IPS_SUBPASSWD_AUTH>(buffer);

            SubpasswordHandler.Instance
                .CreateSubpasswordDataForUserNum(ipsSubpassdAuth.UserNum, ipsSubpassdAuth.Type);

            using var conn = new SqlConnection(Config.Instance.GlobalMgr.ConnectionString);
            var spCabalSubpasswdAuth = await conn.QueryFirstOrDefaultAsync<SP_CABAL_SUBPASSWD_AUTH>("sp_cabal_subpasswd_auth", new
            {
                pUserNum = ipsSubpassdAuth.UserNum,
                pType = ipsSubpassdAuth.Type,
                pPassword = ipsSubpassdAuth.Password.Trim(char.MinValue)
            }, commandType: CommandType.StoredProcedure);

            if (spCabalSubpasswdAuth == null)
            {
                CustomLogger<Subpassword>.Instance
                    .LogError("OnIPC_SUBPASSWD_AUTH(): Failed to auth subpassword! UserNum({0}) Type({1})",
                    ipsSubpassdAuth.UserNum,
                    ipsSubpassdAuth.Type);

                return;
            }

            var ipsResultSubpasswdAuth = new IPS_RESULT_SUBPASSWD_AUTH()
            {
                ToSession = ipsSubpassdAuth.FromSession,
                ToOpcode = ipsSubpassdAuth.FromOpcode,
                Success = spCabalSubpasswdAuth.Success,
                Type = ipsSubpassdAuth.Type
            };

            if (spCabalSubpasswdAuth.Success)
            {
                SubpasswordHandler.Instance
                    .ResetPassFailtCount(ipsSubpassdAuth.UserNum, ipsSubpassdAuth.Type);
            }
            else
            {
                var failCount = SubpasswordHandler.Instance
                    .IncrementAndReturnPassFailCount(
                        ipsSubpassdAuth.UserNum,
                        ipsSubpassdAuth.Type);

                ipsResultSubpasswdAuth.TryNum = failCount;

                if (Config.Instance.GlobalMgr.SubpwdFailBlock && failCount >= Config.Instance.GlobalMgr.SubpwdFailLimit)
                {
                    SubpasswordHandler.Instance
                        .RemoveSubpasswdData(ipsSubpassdAuth.UserNum);

                    await conn.ExecuteAsync("sp_cabal_subpasswd_block", new
                    {
                        pUserNum = ipsSubpassdAuth.UserNum
                    }, commandType: CommandType.StoredProcedure);

                    ipsResultSubpasswdAuth.AccountBlocked = true;
                }
            }

            using var ipsResultSubpasswdAuthS = await IPCSerializer.Instance.SerializeAsync(ipsResultSubpasswdAuth, Share.Opcode.IPC_RESULT_SUBPASSWD_AUTH);
            await session.SendAsync(ipsResultSubpasswdAuthS);
        }

        public static async Task OnIPC_SUBPASSWD_GET_QA(GlobalMgrSession session, ArraySegment<byte> buffer)
        {
            var ipsSubpasswdGetQa = await IPCSerializer.Instance.DeserializeAsync<IPS_SUBPASSWD_GET_QA>(buffer);

            using var conn = new SqlConnection(Config.Instance.GlobalMgr.ConnectionString);
            var spCabalSubpasswdGetQa = await conn.QueryFirstOrDefaultAsync<SP_CABAL_SUBPASSWD_GET_QA>("sp_cabal_subpasswd_get_qa", new
            {
                pUserNum = ipsSubpasswdGetQa.UserNum,
                pType = ipsSubpasswdGetQa.Type
            }, commandType: CommandType.StoredProcedure);

            if (spCabalSubpasswdGetQa == null)
            {
                CustomLogger<Subpassword>.Instance
                    .LogError("OnIPC_SUBPASSWD_GET_QA(): Failed to get QA! UserNum({0}) Type({1})",
                    ipsSubpasswdGetQa.UserNum,
                    ipsSubpasswdGetQa.Type);

                return;
            }

            var ipsResultSubpasswdGetQa = new IPS_RESULT_SUBPASSWD_GET_QA()
            {
                ToSession = ipsSubpasswdGetQa.FromSession,
                Type = ipsSubpasswdGetQa.Type,
                Success = spCabalSubpasswdGetQa.Success,
                QuestionId = spCabalSubpasswdGetQa.QuestionId
            };

            using var ipsResultSubpasswdGetQaS = await IPCSerializer.Instance.SerializeAsync(ipsResultSubpasswdGetQa, Share.Opcode.IPC_RESULT_SUBPASSWD_GET_QA);
            await session.SendAsync(ipsResultSubpasswdGetQaS);
        }

        public static async Task OnIPC_SUBPASSWD_ANSWER_QA(GlobalMgrSession session, ArraySegment<byte> buffer)
        {
            var ipsSubpasswdAnswerQa = await IPCSerializer.Instance.DeserializeAsync<IPS_SUBPASSWD_ANSWER_QA>(buffer);

            SubpasswordHandler.Instance
                .CreateSubpasswordDataForUserNum(ipsSubpasswdAnswerQa.UserNum, ipsSubpasswdAnswerQa.Type);

            using var conn = new SqlConnection(Config.Instance.GlobalMgr.ConnectionString);
            var spCabalSubpasswdAnswerQa = await conn.QueryFirstOrDefaultAsync<SP_CABAL_SUBPASSWD_ANSWER_QA>("sp_cabal_subpasswd_answer_qa", new
            {
                pUserNum = ipsSubpasswdAnswerQa.UserNum,
                pType = ipsSubpasswdAnswerQa.Type,
                pQuestion = ipsSubpasswdAnswerQa.QuestionId,
                pAnswer = ipsSubpasswdAnswerQa.Answer.Trim(char.MinValue).ToLower()
            }, commandType: CommandType.StoredProcedure);

            if (spCabalSubpasswdAnswerQa == null)
            {
                CustomLogger<Subpassword>.Instance
                    .LogError("OnIPC_SUBPASSWD_ANSWER_QA(): Failed to answer QA! UserNum({0}) Type({1})",
                    ipsSubpasswdAnswerQa.UserNum,
                    ipsSubpasswdAnswerQa.Type);

                return;
            }

            var ipsResultSubpasswdAnswerQa = new IPS_RESULT_SUBPASSWD_ANSWER_QA()
            {
                ToSession = ipsSubpasswdAnswerQa.FromSession,
                Success = spCabalSubpasswdAnswerQa.Success,
                Type = ipsSubpasswdAnswerQa.Type
            };

            if (spCabalSubpasswdAnswerQa.Success)
            {
                SubpasswordHandler.Instance
                    .ResetAnswerFailCount(ipsSubpasswdAnswerQa.UserNum, ipsSubpasswdAnswerQa.Type);
            }
            else
            {
                var failCount = SubpasswordHandler.Instance
                    .IncrementAndReturnAnswerFailCount(
                        ipsSubpasswdAnswerQa.UserNum,
                        ipsSubpasswdAnswerQa.Type);

                ipsResultSubpasswdAnswerQa.TryNum = (byte)failCount;

                if (Config.Instance.GlobalMgr.SubpwdFailBlock && failCount >= Config.Instance.GlobalMgr.SubpwdFailLimit)
                {
                    SubpasswordHandler.Instance
                        .RemoveSubpasswdData(ipsSubpasswdAnswerQa.UserNum);

                    await conn.ExecuteAsync("sp_cabal_subpasswd_block", new
                    {
                        pUserNum = ipsSubpasswdAnswerQa.UserNum
                    }, commandType: CommandType.StoredProcedure);

                    ipsResultSubpasswdAnswerQa.AccountBlocked = true;
                }
            }

            using var ipsResultSubpasswdAnswerQaS = await IPCSerializer.Instance.SerializeAsync(ipsResultSubpasswdAnswerQa, Share.Opcode.IPC_RESULT_SUBPASSWD_ANSWER_QA);
            await session.SendAsync(ipsResultSubpasswdAnswerQaS);
        }

        public static async Task OnIPC_SUBPASSWD_DEL(GlobalMgrSession session, ArraySegment<byte> buffer)
        {
            var ipsSubpasswdDel = await IPCSerializer.Instance.DeserializeAsync<IPS_SUBPASSWD_DEL>(buffer);

            using var conn = new SqlConnection(Config.Instance.GlobalMgr.ConnectionString);
            var spCabalSubpasswdAnswerQa = await conn.QueryFirstOrDefaultAsync<SP_CABAL_SUBPASSWD_DEL>("sp_cabal_subpasswd_del", new
            {
                pUserNum = ipsSubpasswdDel.UserNum,
                pType = ipsSubpasswdDel.Type
            }, commandType: CommandType.StoredProcedure);

            if (spCabalSubpasswdAnswerQa == null)
            {
                CustomLogger<Subpassword>.Instance
                    .LogError("OnIPC_SUBPASSWD_DEL(): Failed to delete subpassword! UserNum({0}) Type({1})",
                    ipsSubpasswdDel.UserNum,
                    ipsSubpasswdDel.Type);

                return;
            }

            var ipsResultSubpasswdDel = new IPS_RESULT_SUBPASSWD_DEL()
            {
                ToSession = ipsSubpasswdDel.FromSession,
                Success = spCabalSubpasswdAnswerQa.Success,
                Type = ipsSubpasswdDel.Type
            };

            using var ipsResultSubpasswdDelS = await IPCSerializer.Instance.SerializeAsync(ipsResultSubpasswdDel, Share.Opcode.IPC_RESULT_SUBPASSWD_DEL);
            await session.SendAsync(ipsResultSubpasswdDelS);
        }

        public static async Task OnIPC_SUBPASSWD_CHANGE_QA(GlobalMgrSession session, ArraySegment<byte> buffer)
        {
            var ipsSubpasswdChangeQa = await IPCSerializer.Instance.DeserializeAsync<IPS_SUBPASSWD_CHANGE_QA>(buffer);

            using var conn = new SqlConnection(Config.Instance.GlobalMgr.ConnectionString);
            var spCabalSubpasswdChangeQa = await conn.QueryFirstOrDefaultAsync<SP_CABAL_SUBPASSWD_CHANGE_QA>("sp_cabal_subpasswd_change_qa", new
            {
                pUserNum = ipsSubpasswdChangeQa.UserNum,
                pType = ipsSubpasswdChangeQa.Type,
                pQuestion = ipsSubpasswdChangeQa.QuestionId,
                pAnswer = ipsSubpasswdChangeQa.Answer.Trim(char.MinValue).ToLower()
            }, commandType: CommandType.StoredProcedure);

            if (spCabalSubpasswdChangeQa == null)
            {
                CustomLogger<Subpassword>.Instance
                    .LogError("OnIPC_SUBPASSWD_CHANGE_QA(): Failed to change QA! UserNum({0}) Type({0})",
                    ipsSubpasswdChangeQa.UserNum,
                    ipsSubpasswdChangeQa.Type);

                return;
            }

            var ipsResultSubpasswdChangeQa = new IPS_RESULT_SUBPASSWD_CHANGE_QA()
            {
                ToSession = ipsSubpasswdChangeQa.FromSession,
                Success = spCabalSubpasswdChangeQa.Success,
                Type = ipsSubpasswdChangeQa.Type
            };

            using var ipsResultSubpasswdChangeQaS = await IPCSerializer.Instance.SerializeAsync(ipsResultSubpasswdChangeQa, Share.Opcode.IPC_RESULT_SUBPASSWD_CHANGE_QA);
            await session.SendAsync(ipsResultSubpasswdChangeQaS);
        }
    }
}
