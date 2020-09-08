using System;
using System.Buffers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Share.Serde;
using HPT.Common;
using HPT.Logging;
using HPT.Network;
using static Share.Protosdef;
using static Share.Protos.Services;
using System.Net;

namespace WorldSvr
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
                    GroupIdx = Config.Instance.WorldSvr.GroupIdx,
                    ServerIdx = Config.Instance.WorldSvr.ServerIdx
                },
                IP = IPAddress.Parse(Config.Instance.Listen.IP).GetAddressBytes(),
                Port = Config.Instance.Listen.Port,
                MaxConnections = Config.Instance.WorldSvr.MaxPlayers,
                ServiceType = Config.Instance.WorldSvr.ChannelType
            };

            using var ipsAddServiceS = await IPCSerializer.Instance.SerializeAsync(ipsAddService, Share.Opcode.IPC_ADDSERVICE);
            await SendAsync(ipsAddServiceS);

            await base.OnConnect();
        }

        protected override async Task ProcessMessageReceivedAsync(ReadOnlySequence<byte> messageBytes)
        {
            try
            {
                var cpMessageBytes = messageBytes.ToArray(); // TODO: Maybe pool array

                var ipsHeader = await IPCSerializer.Instance.DeserializeAsync<IPS_HEADER>(cpMessageBytes);
                if (Procdef.GlobalMgrProcMethodHandler.TryGet(ipsHeader.Opcode, out var globalMgrProc))
                {
                    _ = globalMgrProc(cpMessageBytes)
                            .ContinueWith(t => 
                            {
                                if (t.IsFaulted)
                                    throw t.Exception.InnerException;
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
