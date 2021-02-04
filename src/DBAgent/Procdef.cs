using DBAgent.Procs;
using HPT.Handlers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DBAgent
{
    static class Procdef
    {
        public delegate Task DBAgentProc(DBAgentSession session, ArraySegment<byte> buffer);

        public static MethodHandler<ushort, DBAgentProc> DBAgentProcMethodHandler = MethodHandler<ushort, DBAgentProc>.Instance;

        public static void RegisterProcs()
        {

            DBAgentProcMethodHandler

                // CharLobby
                .Register(Share.Opcode.IPC_GETCHARS, CharLobby.OnIPC_GETCHARS)
                .Register(Share.Opcode.IPC_NEWCHAR, CharLobby.OnIPC_NEWCHAR)
                .Register(Share.Opcode.IPC_CHAR_SLOTORDER, CharLobby.OnIPC_CHAR_SLOTORDER)
                .Register(Share.Opcode.IPC_DELCHAR, CharLobby.OnIPC_DELCHAR)
                // End CharLobby

                // Character
                .Register(Share.Opcode.IPC_GETMYCHARACTER, Character.OnIPC_GETMYCHARACTER)
                // End Character
                
                
                ;
        }
    }
}
