using System;
using System.Buffers;
using System.Net.Sockets;
using System.Threading.Tasks;
using GlobalMgrSvr.System;
using HPT.Logging;
using HPT.Network;
using Microsoft.Extensions.Logging;
using Share.Serde;
using static Share.Protosdef;

namespace GlobalMgrSvr
{
    class GlobalMgrSession : TcpSession
    {
        public ServiceContext ServiceContext { get; }

        public GlobalMgrSession(Socket socket, ushort sessionId) : base(socket, sessionId)
        {
            ServiceContext = new ServiceContext(this);
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
                if (ipsHeader != null && Procdef.GlobalMgrServerMethodHandler.TryGet(ipsHeader.Opcode, out var globalMgrProc))
                {
                    await globalMgrProc(this, cpMessageBytes);
                }
            }
            catch (Exception ex)
            {
                CustomLogger<GlobalMgrSession>.Instance
                    .LogError(ex, $"Session({Signature}) raised an exception");
            }
        }
    }
}
