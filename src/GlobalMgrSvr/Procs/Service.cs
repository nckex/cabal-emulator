using System;
using System.Threading.Tasks;
using Share.Serde;
using GlobalMgrSvr.System;
using HPT.Logging;
using Microsoft.Extensions.Logging;
using static Share.Protos.Services;
using System.Collections.Generic;

namespace GlobalMgrSvr.Procs
{
    class Service
    {
        public static async Task OnIPC_ADDSERVICE(GlobalMgrSession session, ArraySegment<byte> buffer)
        {
            var ipsAddService = await IPCSerializer.Instance.DeserializeAsync<IPS_ADDSERVICE>(buffer);

            session.ServiceContext
                .UpdateServiceInfos(
                    ipsAddService.Service.GroupIdx, 
                    ipsAddService.Service.ServerIdx,
                    ipsAddService.IP,
                    ipsAddService.Port,
                    ipsAddService.MaxConnections,
                    ipsAddService.ServiceType);

            var isServerAdded = ServiceStateHandler.Instance.TryAdd(session.ServiceContext);
            if (!isServerAdded)
            {
                CustomLogger<Service>.Instance
                    .LogError($"Failed to register GroupIdx({ipsAddService.Service.GroupIdx}) ServerIdx({ipsAddService.Service.ServerIdx}) service");
            } 
            else
            {
                CustomLogger<Service>.Instance
                    .LogInformation($"Service GroupIdx({ipsAddService.Service.GroupIdx}) ServerIdx({ipsAddService.Service.ServerIdx}) registered");
            }

            var ipsResultAddService = new IPS_RESULT_ADDSERVICE()
            {
                Service = ipsAddService.Service,
                Success = isServerAdded
            };

            using var ipsResultAddServiceS = await IPCSerializer.Instance.SerializeAsync(ipsResultAddService, Share.Opcode.IPC_RESULT_ADDSERVICE);
            await session.SendAsync(ipsResultAddServiceS);
        }

        public static async Task OnIPC_SERVICESTATE(GlobalMgrSession session, ArraySegment<byte> buffer)
        {
            // var ipsServiceState = await IPCSerializer.Instance.DeserializeAsync<IPS_SERVICESTATE>(buffer);

            var serviceState = ServiceStateHandler.Instance.GetAll();

            var serviceServers = new List<IPS_RESULT_SERVICESTATE_SERVER>(serviceState.Length);

            foreach (var service in serviceState)
            {
                var serviceChannels = new List<IPS_RESULT_SERVICESTATE_CHANNEL>(service.Value.Length);
                foreach (var serviceChannel in service.Value)
                {
                    serviceChannels.Add(new IPS_RESULT_SERVICESTATE_CHANNEL()
                    {
                        GroupIdx = serviceChannel.Value.GroupIdx,
                        ServerIdx = serviceChannel.Value.ServerIdx,
                        PublicIP = serviceChannel.Value.IP,
                        PublicPort = serviceChannel.Value.Port,
                        MaxPlayers = serviceChannel.Value.MaxConnections,
                        ServiceType = serviceChannel.Value.Type,
                        OnlinePlayers = serviceChannel.Value.OnlineCount
                    });
                }

                serviceServers.Add(new IPS_RESULT_SERVICESTATE_SERVER()
                {
                    GroupIdx = service.Key,
                    ChannelCount = (byte)serviceChannels.Count,
                    Channels = serviceChannels
                });
            }

            var ipsResultServiceState = new IPS_RESULT_SERVICESTATE()
            {
                ServerCount = (byte)serviceServers.Count,
                Servers = serviceServers
            };

            using var ipsResultServiceStateS = await IPCSerializer.Instance.SerializeAsync(ipsResultServiceState, Share.Opcode.IPC_RESULT_SERVICESTATE);
            await session.SendAsync(ipsResultServiceStateS);
        }
    }
}
