using System;
using System.Buffers;
using System.Net.Sockets;
using System.Threading.Tasks;
using Share.Serde;
using HPT.Network;
using static Share.Protosdef;
using HPT.Logging;
using Microsoft.Extensions.Logging;

namespace DBAgent
{
    class DBAgentSession : TcpSession
    {
        public DBAgentSession(Socket socket, ushort sessionId) : base(socket, sessionId)
        {

        }

        protected override int GetMessageSize(in ReadOnlySequence<byte> headerBytes)
        {
            return BitConverter.ToUInt16(headerBytes.FirstSpan);
        }

        protected override async Task ProcessMessageReceivedAsync(ReadOnlySequence<byte> messageBytes)
        {
            try
            {
                var cpMessageBytes = messageBytes.ToArray(); // TODO: Maybe pool array

                var ipsHeader = await IPCSerializer.Instance.DeserializeAsync<IPS_HEADER>(cpMessageBytes);
                if (ipsHeader != null && Procdef.DBAgentProcMethodHandler.TryGet(ipsHeader.Opcode, out var dbAgentProc))
                {
                    await dbAgentProc(this, cpMessageBytes);
                }
            }
            catch (Exception ex)
            {
                CustomLogger<DBAgentSession>.Instance
                    .LogError(ex, $"Session({Signature}) raised an exception");
            }
        }
    }
}
