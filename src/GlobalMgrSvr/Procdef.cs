using GlobalMgrSvr.Procs;
using HPT.Handlers;
using System;
using System.Threading.Tasks;

namespace GlobalMgrSvr
{
    static class Procdef
    {
        public delegate Task GlobalMgrProc(GlobalMgrSession session, ArraySegment<byte> buffer);

        public static MethodHandler<ushort, GlobalMgrProc> GlobalMgrServerMethodHandler = MethodHandler<ushort, GlobalMgrProc>.Instance;

        public static void RegisterProcs()
        {
            GlobalMgrServerMethodHandler

                // Service
                .Register(Share.Opcode.IPC_ADDSERVICE, Service.OnIPC_ADDSERVICE)
                .Register(Share.Opcode.IPC_SERVICESTATE, Service.OnIPC_SERVICESTATE)
                // End Service

                // Connect
                .Register(Share.Opcode.IPC_LINK, Connect.OnIPC_LINK)
                .Register(Share.Opcode.IPC_RESULT_LINK, Connect.OnIPC_RESULT_LINK)
                .Register(Share.Opcode.IPC_RESULT_FDISCONNECT, Connect.OnIPC_RESULT_FDISCONNECT)
                // End Connect

                // Auth
                .Register(Share.Opcode.IPC_AUTH, Auth.OnIPC_AUTH)
                .Register(Share.Opcode.IPC_LOGINSTATE, Auth.OnIPC_LOGINSTATE)
                .Register(Share.Opcode.IPC_REQ_FDISCONNECT, Auth.OnIPC_REQ_FDISCONNECT)
                // End Auth

                // CharLobby
                .Register(Share.Opcode.IPC_CHECKPASSWD, CharLobby.OnIPC_CHECKPASSWD)
                // End CharLobby

                // Subpassword
                .Register(Share.Opcode.IPC_SUBPASSWD_ASK, Subpassword.OnIPC_SUBPASSWD_ASK)
                .Register(Share.Opcode.IPC_SUBPASSWD_SET, Subpassword.OnIPC_SUBPASSWD_SET)
                .Register(Share.Opcode.IPC_SUBPASSWD_AUTH_REMEMBER, Subpassword.OnIPC_SUBPASSWD_AUTH_REMEMBER)
                .Register(Share.Opcode.IPC_SUBPASSWD_AUTH, Subpassword.OnIPC_SUBPASSWD_AUTH)
                .Register(Share.Opcode.IPC_SUBPASSWD_GET_QA, Subpassword.OnIPC_SUBPASSWD_GET_QA)
                .Register(Share.Opcode.IPC_SUBPASSWD_ANSWER_QA, Subpassword.OnIPC_SUBPASSWD_ANSWER_QA)
                .Register(Share.Opcode.IPC_SUBPASSWD_DEL, Subpassword.OnIPC_SUBPASSWD_DEL)
                .Register(Share.Opcode.IPC_SUBPASSWD_CHANGE_QA, Subpassword.OnIPC_SUBPASSWD_CHANGE_QA)
                // End Subpassword





                ;
        }
    }
}
