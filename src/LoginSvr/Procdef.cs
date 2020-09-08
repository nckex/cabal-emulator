using System;
using System.Threading.Tasks;
using HPT.Handlers;
using LoginSvr.Procs;

namespace LoginSvr
{
    static class Procdef
    {
        public delegate Task LoginSvrProc(LoginSvrSession loginSvrSession, ArraySegment<byte> buffer);
        public delegate Task GlobalMgrProc(ArraySegment<byte> buffer);

        public static MethodHandler<ushort, LoginSvrProc> LoginServerMethodHandler = MethodHandler<ushort, LoginSvrProc>.Instance;
        public static MethodHandler<ushort, GlobalMgrProc> GlobalMgrClientMethodHandler = MethodHandler<ushort, GlobalMgrProc>.Instance;

        public static void RegisterProcs()
        {
            LoginServerMethodHandler

                // Connect
                .Register(Opcode.CSC_CONNECT2SERV, Connect.OnC2S_CONNECT2SERV)
                .Register(Opcode.CSC_CHECKVERSION, Connect.OnC2S_CHECKVERSION)
                .Register(Opcode.CSC_VERIFYLINKS, Connect.OnC2S_VERIFYLINKS)
                // End Connect

                // Auth
                .Register(Opcode.CSC_PRE_AUTHENTICATE, Auth.OnC2S_PRE_AUTHENTICATE)
                .Register(Opcode.CSC_RSA_PUBLIC_KEY, Auth.OnC2S_RSA_PUBLIC_KEY)
                .Register(Opcode.CSC_PREAUTH_STATUS, Auth.OnC2S_PREAUTH_STATUS)
                .Register(Opcode.CSC_AUTHENTICATE, Auth.OnC2S_AUTHENTICATE)
                .Register(Opcode.CSC_FDISCONNECT, Auth.OnC2S_FDISCONNECT)
                // End Auth













                // Unused
                .Register(Opcode.UNUSED_3383, Unused.OnUNUSED_3383)
                // End Unused
                ;

            GlobalMgrClientMethodHandler

                // Service
                .Register(Share.Opcode.IPC_RESULT_ADDSERVICE, Service.OnIPC_RESULT_ADDSERVICE)
                .Register(Share.Opcode.IPC_RESULT_SERVICESTATE, Service.OnIPC_RESULT_SERVICESTATE)
                // End Service

                // Auth
                .Register(Share.Opcode.IPC_RESULT_AUTH, Auth.OnIPC_RESULT_AUTH)
                .Register(Share.Opcode.IPC_RESULT_LOGINSTATE, Auth.OnIPC_RESULT_LOGINSTATE)
                .Register(Share.Opcode.IPC_RESULT_REQ_FDISCONNECT, Auth.OnIPC_RESULT_REQ_FDISCONNECT)
                // End Auth

                // Connect
                .Register(Share.Opcode.IPC_LINK, Connect.OnIPC_LINK)
                .Register(Share.Opcode.IPC_RESULT_LINK, Connect.OnIPC_RESULT_LINK)
                .Register(Share.Opcode.IPC_FDISCONNECT, Connect.OnIPC_FDISCONNECT)
                // End Connect

                ;
        }
    }
}
