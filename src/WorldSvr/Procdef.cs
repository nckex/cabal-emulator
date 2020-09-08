using System;
using System.Threading.Tasks;
using WorldSvr.Procs;
using HPT.Handlers;

namespace WorldSvr
{
    class Procdef
    {
        public delegate Task WorldSvrProc(WorldSvrSession session, ArraySegment<byte> buffer);
        public delegate Task GlobalMgrProc(ArraySegment<byte> buffer);
        public delegate Task DBAgentProc(ArraySegment<byte> buffer);

        public static MethodHandler<ushort, WorldSvrProc> WorldSvrProcMethodHandler = MethodHandler<ushort, WorldSvrProc>.Instance;
        public static MethodHandler<ushort, GlobalMgrProc> GlobalMgrProcMethodHandler = MethodHandler<ushort, GlobalMgrProc>.Instance;
        public static MethodHandler<ushort, DBAgentProc> DBAgentProcMethodHandler = MethodHandler<ushort, DBAgentProc>.Instance;

        public static void RegisterProcs()
        {
            WorldSvrProcMethodHandler

                // Connect
                .Register(Opcode.CSC_CONNECT2SERV, Connect.OnC2S_CONNECT2SERV)
                .Register(Opcode.CSC_VERIFYLINKS, Connect.OnC2S_VERIFYLINKS)
                // End Connect

                // WorldServer
                .Register(Opcode.CSC_GETSVRTIME, WorldServer.OnC2S_GETSVRTIME)
                .Register(Opcode.CSC_SERVERENV, WorldServer.OnC2S_SERVERENV)
                .Register(Opcode.CSC_PING, WorldServer.OnC2S_PING)
                // End WorldServer

                // Charge
                .Register(Opcode.CSC_CHARGEINFO, Charge.OnC2S_CHARGEINFO)
                // End Charge

                // CharLobby
                .Register(Opcode.CSC_GETMYCHARTR, CharLobby.OnC2S_GETMYCHARTR)
                .Register(Opcode.CSC_SPECIALCHAREVT, CharLobby.OnC2S_SPECIALCHAREVT)
                .Register(Opcode.CSC_NEWMYCHARTR, CharLobby.OnC2S_NEWMYCHARTR)
                .Register(Opcode.CSC_CHARACTER_SLOTORDER, CharLobby.OnC2S_CHARACTER_SLOTORDER)
                .Register(Opcode.CSC_CHECKPASSWD, CharLobby.OnC2S_CHECKPASSWD)
                .Register(Opcode.CSC_DELMYCHARTR, CharLobby.OnC2S_DELMYCHARTR)
                // End CharLobby

                // Subpassword
                .Register(Opcode.CSC_SUBPASSWORD_SET, Subpassword.OnC2S_SUBPASSWD_SET)
                .Register(Opcode.CSC_SUBPASSWORD_ASK, Subpassword.OnC2S_SUBPASSWORD_ASK)
                .Register(Opcode.CSC_SUBPASSWORD_AUTH, Subpassword.OnC2S_SUBPASSWORD_AUTH_REMEMBER)
                .Register(Opcode.CSC_SUBPASSWORD_GET_QA, Subpassword.OnC2S_SUBPASSWORD_GET_QA)
                .Register(Opcode.CSC_SUBPASSWORD_ANSWER_QA, Subpassword.OnC2S_SUBPASSWORD_ANSWER_QA)
                .Register(Opcode.CSC_SUBPASSWORD_DEL_AUTH, Subpassword.OnC2S_SUBPASSWORD_DEL_AUTH)
                .Register(Opcode.CSC_SUBPASSWORD_DEL, Subpassword.OnC2S_SUBPASSWORD_DEL)
                .Register(Opcode.CSC_SUBPASSWORD_CHANGE_QA_AUTH, Subpassword.OnC2S_SUBPASSWORD_CHANGE_QA_AUTH)
                .Register(Opcode.CSC_SUBPASSWORD_CHANGE_QA, Subpassword.OnC2S_SUBPASSWORD_CHANGE_QA)
                // End Subpassword

                // Character
                .Register(Opcode.CSC_INITIALIZED, Character.OnC2S_INITIALIZED)
                // End Character

            ;

            GlobalMgrProcMethodHandler

                // Connect
                .Register(Share.Opcode.IPC_LINK, Connect.OnIPC_LINK)
                .Register(Share.Opcode.IPC_RESULT_LINK, Connect.OnIPC_RESULT_LINK)
                .Register(Share.Opcode.IPC_FDISCONNECT, Connect.OnIPC_FDISCONNECT)
                // End Connect

                // CharLobby
                .Register(Share.Opcode.IPC_RESULT_CHECKPASSWD, CharLobby.OnIPC_RESULT_CHECKPASSWD)
                // End CharLobby

                // Subpassword
                .Register(Share.Opcode.IPC_RESULT_SUBPASSWD_ASK, Subpassword.OnIPC_RESULT_SUBPASSWD_ASK)
                .Register(Share.Opcode.IPC_RESULT_SUBPASSWD_SET, Subpassword.OnIPC_RESULT_SUBPASSWD_SET)
                .Register(Share.Opcode.IPC_RESULT_SUBPASSWD_AUTH_REMEMBER, Subpassword.OnIPC_RESULT_SUBPASSWD_AUTH_REMEMBER)
                .Register(Share.Opcode.IPC_RESULT_SUBPASSWD_AUTH, Subpassword.OnIPC_RESULT_SUBPASSWD_AUTH)
                .Register(Share.Opcode.IPC_RESULT_SUBPASSWD_GET_QA, Subpassword.OnIPC_RESULT_SUBPASSWD_GET_QA)
                .Register(Share.Opcode.IPC_RESULT_SUBPASSWD_ANSWER_QA, Subpassword.OnIPC_RESULT_SUBPASSWD_ANSWER_QA)
                .Register(Share.Opcode.IPC_RESULT_SUBPASSWD_DEL, Subpassword.OnIPC_RESULT_SUBPASSWD_DEL)
                .Register(Share.Opcode.IPC_RESULT_SUBPASSWD_CHANGE_QA, Subpassword.OnIPC_RESULT_SUBPASSWD_CHANGE_QA)
                // End Subpassword

                ;

            DBAgentProcMethodHandler

                // CharLobby
                .Register(Share.Opcode.IPC_RESULT_GETCHARS, CharLobby.OnIPC_RESULT_GETCHARS)
                .Register(Share.Opcode.IPC_RESULT_NEWCHAR, CharLobby.OnIPC_RESULT_NEWCHAR)
                .Register(Share.Opcode.IPC_RESULT_CHAR_SLOTORDER, CharLobby.OnIPC_RESULT_CHAR_SLOTORDER)
                .Register(Share.Opcode.IPC_RESULT_DELCHAR, CharLobby.OnIPC_RESULT_DELCHAR)
                // End CharLobby

                // Character
                .Register(Share.Opcode.IPC_RESULT_GETMYCHARACTER, Character.OnIPC_RESULT_GETMYCHARACTER)
                // End Character

                ;
        }
    }
}
