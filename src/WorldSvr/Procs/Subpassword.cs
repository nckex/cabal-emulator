using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Share.Serde;
using HPT.Logging;
using static WorldSvr.Protos.Subpassword;
using static Share.Protos.Subpassword;
using static Share.Protosdef;
using static Share.Protodef;

namespace WorldSvr.Procs
{
    class Subpassword
    {
        public static async Task OnC2S_SUBPASSWD_SET(WorldSvrSession session, ArraySegment<byte> buffer)
        {
            var c2sSubpasswdSet = await CabalSerializer.Instance.DeserializeAsync<C2S_SUBPASSWORD_SET>(buffer);

            // TODO: Mode validation logic

            var ipsSubpasswdSet = new IPS_SUBPASSWD_SET()
            {
                FromSession = new IPS_SESSION()
                {
                    SessionId = session.SessionId,
                    SessionTime = session.SessionTime
                },
                UserNum = session.CharUserContext.UserNum,
                Type = c2sSubpasswdSet.Type,
                Mode = c2sSubpasswdSet.Mode,
                Password = c2sSubpasswdSet.Password,
                Question = c2sSubpasswdSet.Question,
                Answer = c2sSubpasswdSet.Answer
            };

            using var ipsSubpasswdSetS = await IPCSerializer.Instance.SerializeAsync(ipsSubpasswdSet, Share.Opcode.IPC_SUBPASSWD_SET);
            await GlobalMgrClient.Instance.SendAsync(ipsSubpasswdSetS);
        }

        public static async Task OnIPC_RESULT_SUBPASSWD_SET(ArraySegment<byte> buffer)
        {
            var ipsResultSubpasswdSet = await IPCSerializer.Instance.DeserializeAsync<IPS_RESULT_SUBPASSWD_SET>(buffer);

            if (!WorldSvrServer.Instance.TryGetSession(ipsResultSubpasswdSet.ToSession.SessionId, ipsResultSubpasswdSet.ToSession.SessionTime, out var session))
                return;

            if (ipsResultSubpasswdSet.Success)
            {
                switch (ipsResultSubpasswdSet.Type)
                {
                    case SubpasswordType.Account:
                        session.CharUserContext.UseACSUB = true;
                        break;
                    case SubpasswordType.Wharehouse:
                        session.CharUserContext.UseWHSUB = true;
                        break;
                    case SubpasswordType.Equipment:
                        session.CharUserContext.UseEQSUB = true;
                        break;
                }
            }

            var s2cSubpasswdSet = new S2C_SUBPASSWORD_SET()
            {
                Success = ipsResultSubpasswdSet.Success,
                Mode = ipsResultSubpasswdSet.Mode,
                Type = ipsResultSubpasswdSet.Type
            };

            using var s2cSubpasswdSetS = await CabalSerializer.Instance.SerializeAsync(s2cSubpasswdSet, Opcode.CSC_SUBPASSWORD_SET);
            await session.SendAsync(s2cSubpasswdSetS);
        }

        public static async Task OnC2S_SUBPASSWORD_ASK(WorldSvrSession session, ArraySegment<byte> _)
        {
            var ipsSubpasswdAsk = new IPS_SUBPASSWD_ASK()
            {
                FromSession = new IPS_SESSION()
                {
                    SessionId = session.SessionId,
                    SessionTime = session.SessionTime
                },
                UserNum = session.CharUserContext.UserNum,
                IpAddress = session.IPAddress32
            };

            using var ipsSubpasswdAskS = await IPCSerializer.Instance.SerializeAsync(ipsSubpasswdAsk, Share.Opcode.IPC_SUBPASSWD_ASK);
            await GlobalMgrClient.Instance.SendAsync(ipsSubpasswdAskS);
        }

        public static async Task OnIPC_RESULT_SUBPASSWD_ASK(ArraySegment<byte> buffer)
        {
            var ipsResultSubpasswdAsk = await IPCSerializer.Instance.DeserializeAsync<IPS_RESULT_SUBPASSWD_ASK>(buffer);

            if (!WorldSvrServer.Instance.TryGetSession(ipsResultSubpasswdAsk.ToSession.SessionId, ipsResultSubpasswdAsk.ToSession.SessionTime, out var session))
                return;

            var s2cSubpasswordAsk = new S2C_SUBPASSWORD_ASK()
            {
                IsRequired = session.CharUserContext.UseACSUB && ipsResultSubpasswdAsk.IsRequired
            };

            using var s2cSubpasswordAskS = await CabalSerializer.Instance.SerializeAsync(s2cSubpasswordAsk, Opcode.CSC_SUBPASSWORD_ASK);
            await session.SendAsync(s2cSubpasswordAskS);
        }

        public static async Task OnC2S_SUBPASSWORD_AUTH_REMEMBER(WorldSvrSession session, ArraySegment<byte> buffer)
        {
            var c2sSubpasswordAuthRemember = await CabalSerializer.Instance.DeserializeAsync<C2S_SUBPASSWORD_AUTH_REMEMBER>(buffer);

            var ipsSubpasswdAuthRemember = new IPS_SUBPASSWD_AUTH_REMEMBER()
            {
                FromSession = new IPS_SESSION()
                {
                    SessionId = session.SessionId,
                    SessionTime = session.SessionTime
                },
                FromOpcode = c2sSubpasswordAuthRemember.Opcode,
                UserNum = session.CharUserContext.UserNum,
                Password = c2sSubpasswordAuthRemember.Password,
                Type = c2sSubpasswordAuthRemember.Type,
                RememberHours = c2sSubpasswordAuthRemember.RememberHours,
                IPAddress32 = session.IPAddress32
            };

            using var ipsSubpasswdAuthRememberS = await IPCSerializer.Instance.SerializeAsync(ipsSubpasswdAuthRemember, Share.Opcode.IPC_SUBPASSWD_AUTH_REMEMBER);
            await GlobalMgrClient.Instance.SendAsync(ipsSubpasswdAuthRememberS);
        }

        public static async Task OnIPC_RESULT_SUBPASSWD_AUTH_REMEMBER(ArraySegment<byte> buffer)
        {
            var ipsResultSubpasswdAuth = await IPCSerializer.Instance.DeserializeAsync<IPS_RESULT_SUBPASSWD_AUTH>(buffer);

            if (!WorldSvrServer.Instance.TryGetSession(ipsResultSubpasswdAuth.ToSession.SessionId, ipsResultSubpasswdAuth.ToSession.SessionTime, out var session))
                return;

            if (ipsResultSubpasswdAuth.AccountBlocked)
            {
                session.Disconnect();
                return;
            }

            var s2cSubpasswordAuthRemember = new S2C_SUBPASSWORD_AUTH_REMEMBER()
            {
                Success = ipsResultSubpasswdAuth.Success,
                Type = ipsResultSubpasswdAuth.Type,
                TryNum = ipsResultSubpasswdAuth.TryNum
            };

            using var s2cSubpasswordAuthRememberS = await CabalSerializer.Instance.SerializeAsync(s2cSubpasswordAuthRemember, ipsResultSubpasswdAuth.ToOpcode);
            await session.SendAsync(s2cSubpasswordAuthRememberS);
        }

        public static async Task OnC2S_SUBPASSWORD_CHANGE_QA_AUTH(WorldSvrSession session, ArraySegment<byte> buffer)
        {
            var c2sSubpasswordAuth = await CabalSerializer.Instance.DeserializeAsync<C2S_SUBPASSWORD_CHANGE_QA_AUTH>(buffer);

            var ipsSubpasswdAuth = new IPS_SUBPASSWD_AUTH()
            {
                FromSession = new IPS_SESSION()
                {
                    SessionId = session.SessionId,
                    SessionTime = session.SessionTime
                },
                FromOpcode = c2sSubpasswordAuth.Opcode,
                UserNum = session.CharUserContext.UserNum,
                Password = c2sSubpasswordAuth.Password,
                Type = c2sSubpasswordAuth.Type
            };

            using var ipsSubpasswdAuthS = await IPCSerializer.Instance.SerializeAsync(ipsSubpasswdAuth, Share.Opcode.IPC_SUBPASSWD_AUTH);
            await GlobalMgrClient.Instance.SendAsync(ipsSubpasswdAuthS);
        }

        public static async Task OnC2S_SUBPASSWORD_DEL_AUTH(WorldSvrSession session, ArraySegment<byte> buffer)
        {
            var c2sSubpasswordAuth = await CabalSerializer.Instance.DeserializeAsync<C2S_SUBPASSWORD_DEL_AUTH>(buffer);

            var ipsSubpasswdAuth = new IPS_SUBPASSWD_AUTH()
            {
                FromSession = new IPS_SESSION()
                {
                    SessionId = session.SessionId,
                    SessionTime = session.SessionTime
                },
                FromOpcode = c2sSubpasswordAuth.Opcode,
                UserNum = session.CharUserContext.UserNum,
                Password = c2sSubpasswordAuth.Password,
                Type = c2sSubpasswordAuth.Type
            };

            using var ipsSubpasswdAuthS = await IPCSerializer.Instance.SerializeAsync(ipsSubpasswdAuth, Share.Opcode.IPC_SUBPASSWD_AUTH);
            await GlobalMgrClient.Instance.SendAsync(ipsSubpasswdAuthS);
        }

        public static async Task OnIPC_RESULT_SUBPASSWD_AUTH(ArraySegment<byte> buffer)
        {
            var ipsResultSubpasswdAuth = await IPCSerializer.Instance.DeserializeAsync<IPS_RESULT_SUBPASSWD_AUTH>(buffer);

            if (!WorldSvrServer.Instance.TryGetSession(ipsResultSubpasswdAuth.ToSession.SessionId, ipsResultSubpasswdAuth.ToSession.SessionTime, out var session))
                return;

            if (ipsResultSubpasswdAuth.AccountBlocked)
            {
                session.Disconnect();
                return;
            }

            var s2cSubpasswordAuth = new S2C_SUBPASSWORD_AUTH()
            {
                Success = ipsResultSubpasswdAuth.Success,
                Type = ipsResultSubpasswdAuth.Type,
                TryNum = ipsResultSubpasswdAuth.TryNum
            };

            using var s2cSubpasswordAuthS = await CabalSerializer.Instance.SerializeAsync(s2cSubpasswordAuth, ipsResultSubpasswdAuth.ToOpcode);
            await session.SendAsync(s2cSubpasswordAuthS);
        }

        public static async Task OnC2S_SUBPASSWORD_GET_QA(WorldSvrSession session, ArraySegment<byte> buffer)
        {
            var c2sSubpasswordGetQa = await CabalSerializer.Instance.DeserializeAsync<C2S_SUBPASSWORD_GET_QA>(buffer);

            var ipsSubpasswordGetQa = new IPS_SUBPASSWD_GET_QA()
            {
                FromSession = new IPS_SESSION()
                {
                    SessionId = session.SessionId,
                    SessionTime = session.SessionTime
                },
                UserNum = session.CharUserContext.UserNum,
                Type = c2sSubpasswordGetQa.Type
            };

            using var ipsSubpasswordGetQaS = await IPCSerializer.Instance.SerializeAsync(ipsSubpasswordGetQa, Share.Opcode.IPC_SUBPASSWD_GET_QA);
            await GlobalMgrClient.Instance.SendAsync(ipsSubpasswordGetQaS);
        }

        public static async Task OnIPC_RESULT_SUBPASSWD_GET_QA(ArraySegment<byte> buffer)
        {
            var ipsResultSubpasswdGetQa = await IPCSerializer.Instance.DeserializeAsync<IPS_RESULT_SUBPASSWD_GET_QA>(buffer);

            if (!WorldSvrServer.Instance.TryGetSession(ipsResultSubpasswdGetQa.ToSession.SessionId, ipsResultSubpasswdGetQa.ToSession.SessionTime, out var session))
                return;

            if (!ipsResultSubpasswdGetQa.Success)
            {
                CustomLogger<Subpassword>.Instance
                    .LogError("OnIPC_RESULT_SUBPASSWD_GET_QA(): Success = false Type({0}) UserNum({1})",
                    (byte)ipsResultSubpasswdGetQa.Type,
                    session.CharUserContext.UserNum);

                return;
            }

            var s2cSubpasswordGetQa = new S2C_SUBPASSWORD_GET_QA()
            {
                QuestionId1 = ipsResultSubpasswdGetQa.QuestionId,
                QuestionId2 = ipsResultSubpasswdGetQa.QuestionId,
                Type = ipsResultSubpasswdGetQa.Type
            };

            using var s2cSubpasswordGetQaS = await CabalSerializer.Instance.SerializeAsync(s2cSubpasswordGetQa, Opcode.CSC_SUBPASSWORD_GET_QA);
            await session.SendAsync(s2cSubpasswordGetQaS);
        }

        public static async Task OnC2S_SUBPASSWORD_ANSWER_QA(WorldSvrSession session, ArraySegment<byte> buffer)
        {
            var c2sSubpasswordAnswerQa = await CabalSerializer.Instance.DeserializeAsync<C2S_SUBPASSWORD_ANSWER_QA>(buffer);

            var ipsSubpasswdAnswerQa = new IPS_SUBPASSWD_ANSWER_QA()
            {
                FromSession = new IPS_SESSION()
                {
                    SessionId = session.SessionId,
                    SessionTime = session.SessionTime
                },
                UserNum = session.CharUserContext.UserNum,
                Type = c2sSubpasswordAnswerQa.Type,
                QuestionId = c2sSubpasswordAnswerQa.QuestionId,
                Answer = c2sSubpasswordAnswerQa.Answer
            };

            using var ipsSubpasswdAnswerQaS = await IPCSerializer.Instance.SerializeAsync(ipsSubpasswdAnswerQa, Share.Opcode.IPC_SUBPASSWD_ANSWER_QA);
            await GlobalMgrClient.Instance.SendAsync(ipsSubpasswdAnswerQaS);
        }

        public static async Task OnIPC_RESULT_SUBPASSWD_ANSWER_QA(ArraySegment<byte> buffer)
        {
            var ipsResultSubpasswdAnswerQa = await IPCSerializer.Instance.DeserializeAsync<IPS_RESULT_SUBPASSWD_ANSWER_QA>(buffer);

            if (!WorldSvrServer.Instance.TryGetSession(ipsResultSubpasswdAnswerQa.ToSession.SessionId, ipsResultSubpasswdAnswerQa.ToSession.SessionTime, out var session))
                return;

            if (ipsResultSubpasswdAnswerQa.AccountBlocked)
            {
                session.Disconnect();
                return;
            }

            var s2cSubpasswordAnswerQa = new S2C_SUBPASSWORD_ANSWER_QA()
            {
                Success = ipsResultSubpasswdAnswerQa.Success,
                TryNum = ipsResultSubpasswdAnswerQa.TryNum,
                Type = ipsResultSubpasswdAnswerQa.Type
            };

            using var s2cSubpasswordAnswerQaS = await CabalSerializer.Instance.SerializeAsync(s2cSubpasswordAnswerQa, Opcode.CSC_SUBPASSWORD_ANSWER_QA);
            await session.SendAsync(s2cSubpasswordAnswerQaS);
        }

        public static async Task OnC2S_SUBPASSWORD_DEL(WorldSvrSession session, ArraySegment<byte> buffer)
        {
            var c2sSubpasswordDel = await CabalSerializer.Instance.DeserializeAsync<C2S_SUBPASSWORD_DEL>(buffer);

            var ipsSubpasswdDel = new IPS_SUBPASSWD_DEL()
            {
                FromSession = new IPS_SESSION()
                {
                    SessionId = session.SessionId,
                    SessionTime = session.SessionTime
                },
                UserNum = session.CharUserContext.UserNum,
                Type = c2sSubpasswordDel.Type
            };

            using var ipsSubpasswdDelS = await IPCSerializer.Instance.SerializeAsync(ipsSubpasswdDel, Share.Opcode.IPC_SUBPASSWD_DEL);
            await GlobalMgrClient.Instance.SendAsync(ipsSubpasswdDelS);
        }

        public static async Task OnIPC_RESULT_SUBPASSWD_DEL(ArraySegment<byte> buffer)
        {
            var ipsResultSubpasswdDel = await IPCSerializer.Instance.DeserializeAsync<IPS_RESULT_SUBPASSWD_DEL>(buffer);

            if (!WorldSvrServer.Instance.TryGetSession(ipsResultSubpasswdDel.ToSession.SessionId, ipsResultSubpasswdDel.ToSession.SessionTime, out var session))
                return;

            if (ipsResultSubpasswdDel.Success)
            {
                switch (ipsResultSubpasswdDel.Type)
                {
                    case SubpasswordType.Account:
                        session.CharUserContext.UseACSUB = false;
                        break;

                    case SubpasswordType.Wharehouse:
                        session.CharUserContext.UseWHSUB = false;
                        break;

                    case SubpasswordType.Equipment:
                        session.CharUserContext.UseEQSUB = false;
                        break;
                }
            }

            var s2cSubpasswordDel = new S2C_SUBPASSWORD_DEL()
            {
                Success = ipsResultSubpasswdDel.Success,
                Type = ipsResultSubpasswdDel.Type
            };

            using var s2cSubpasswordDelS = await CabalSerializer.Instance.SerializeAsync(s2cSubpasswordDel, Opcode.CSC_SUBPASSWORD_DEL);
            await session.SendAsync(s2cSubpasswordDelS);
        }

        public static async Task OnC2S_SUBPASSWORD_CHANGE_QA(WorldSvrSession session, ArraySegment<byte> buffer)
        {
            var c2sSubpasswordChangeQa = await CabalSerializer.Instance.DeserializeAsync<C2S_SUBPASSWORD_CHANGE_QA>(buffer);

            var ipsSubpasswdChangeQa = new IPS_SUBPASSWD_CHANGE_QA()
            {
                FromSession = new IPS_SESSION()
                {
                    SessionId = session.SessionId,
                    SessionTime = session.SessionTime
                },
                UserNum = session.CharUserContext.UserNum,
                Type = c2sSubpasswordChangeQa.Type,
                QuestionId = c2sSubpasswordChangeQa.QuestionId,
                Answer = c2sSubpasswordChangeQa.Answer
            };

            using var ipsSubpasswdChangeQaS = await IPCSerializer.Instance.SerializeAsync(ipsSubpasswdChangeQa, Share.Opcode.IPC_SUBPASSWD_CHANGE_QA);
            await GlobalMgrClient.Instance.SendAsync(ipsSubpasswdChangeQaS);
        }

        public static async Task OnIPC_RESULT_SUBPASSWD_CHANGE_QA(ArraySegment<byte> buffer)
        {
            var ipsResultSubpasswdChangeQa = await IPCSerializer.Instance.DeserializeAsync<IPS_RESULT_SUBPASSWD_CHANGE_QA>(buffer);

            if (!WorldSvrServer.Instance.TryGetSession(ipsResultSubpasswdChangeQa.ToSession.SessionId, ipsResultSubpasswdChangeQa.ToSession.SessionTime, out var session))
                return;

            var s2cSubpasswordChangeQa = new S2C_SUBPASSWORD_CHANGE_QA()
            {
                Success = ipsResultSubpasswdChangeQa.Success,
                Type = ipsResultSubpasswdChangeQa.Type
            };

            using var s2cSubpasswordChangeQaS = await CabalSerializer.Instance.SerializeAsync(s2cSubpasswordChangeQa, Opcode.CSC_SUBPASSWORD_CHANGE_QA);
            await session.SendAsync(s2cSubpasswordChangeQaS);
        }
    }
}
