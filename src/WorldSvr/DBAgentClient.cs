using System;
using System.Buffers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Share.Serde;
using HPT.Common;
using HPT.Logging;
using HPT.Network;
using static Share.Protosdef;

namespace WorldSvr
{
    class DBAgentClient : TcpClient
    {
        public static DBAgentClient Instance => Singleton<DBAgentClient>.I;

        protected override ILogger<TcpClient> CreateLogger()
        {
            return CustomLogger<DBAgentClient>.Instance;
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
                if (Procdef.DBAgentProcMethodHandler.TryGet(ipsHeader.Opcode, out var globalMgrProc))
                {
                    _ = globalMgrProc(cpMessageBytes)
                            .ContinueWith(t => 
                            {
                                if (t.IsFaulted)
                                    _logger.LogError(t.Exception.InnerException, "ProcessMessageReceivedAsync() exception");
                            });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProcessMessageReceivedAsync() exception");
            }
        }
    }
}
