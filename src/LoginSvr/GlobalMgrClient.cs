using System;
using System.Buffers;
using System.Threading.Tasks;
using HPT.Common;
using HPT.Logging;
using HPT.Network;
using LoginSvr.System;
using Microsoft.Extensions.Logging;
using Share.Serde;
using static Share.Protos.Services;
using static Share.Protosdef;

namespace LoginSvr
{
    class GlobalMgrClient : TcpClient
    {
        public static GlobalMgrClient Instance => Singleton<GlobalMgrClient>.I;

        protected override ILogger<TcpClient> CreateLogger()
        {
            return CustomLogger<GlobalMgrClient>.Instance;
        }

        protected override int GetMessageSize(in ReadOnlySequence<byte> headerBytes)
        {
            return BitConverter.ToUInt16(headerBytes.FirstSpan);
        }

        protected override async Task OnConnect()
        {
            var ipsAddService = new IPS_ADDSERVICE()
            {
                Service = new IPS_SERVICE()
                {
                    ServerIdx = Config.Instance.LoginSvr.ServerIdx,
                    GroupIdx = Protodef.LOGINSVR_GROUPIDX
                }
            };

            using var ipsAddServiceS = await IPCSerializer.Instance.SerializeAsync(ipsAddService, Share.Opcode.IPC_ADDSERVICE);
            await SendAsync(ipsAddServiceS);

            await base.OnConnect();
        }

        protected override async Task OnDisconnect()
        {
            ServiceStateHandler.Instance.StopPool();

            await base.OnDisconnect();
        }

        protected override async Task ProcessMessageReceivedAsync(ReadOnlySequence<byte> messageBytes)
        {
            try
            {
                var cpMessageBytes = messageBytes.ToArray(); // TODO: Maybe pool array

                var ipsHeader = await IPCSerializer.Instance.DeserializeAsync<IPS_HEADER>(cpMessageBytes);
                if (ipsHeader != null && Procdef.GlobalMgrClientMethodHandler.TryGet(ipsHeader.Opcode, out var globalMgrProc))
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
