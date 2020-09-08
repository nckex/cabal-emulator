using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LoginSvr.System;
using Share.Serde;
using static LoginSvr.Protos.ServerState;
using static Share.Protos.Services;

namespace LoginSvr.Procs
{
    class Service
    {
        public static async Task OnIPC_RESULT_ADDSERVICE(ArraySegment<byte> buffer)
        {
            var ipsResultAddService = await IPCSerializer.Instance.DeserializeAsync<IPS_RESULT_ADDSERVICE>(buffer);
            if (ipsResultAddService.Success)
            {
                ServiceStateHandler.Instance.StartPool(); // Start serverstate pool
            }
        }

        public static async Task OnIPC_RESULT_SERVICESTATE(ArraySegment<byte> buffer)
        {
            var ipsResultServiceState = await IPCSerializer.Instance.DeserializeAsync<IPS_RESULT_SERVICESTATE>(buffer);

            var nfsServers = ipsResultServiceState.Servers
                .Where(s => s.GroupIdx != Protodef.LOGINSVR_GROUPIDX)
                .Select(
                s => new NFS_SERVERSTATE_SERVER()
                {
                    GroupIdx = s.GroupIdx,
                    ChannelCount = s.ChannelCount,
                    Channels = s.Channels.Select(
                        c => new NFS_SERVERSTATE_SERVER_CHANNEL() 
                        {
                            GroupIdx = c.GroupIdx,
                            ServerIdx = c.ServerIdx,
                            OnlinePlayers = c.OnlinePlayers,
                            MaxPlayers = c.MaxPlayers,
                            PublicIP = new IPAddress(c.PublicIP).ToString(),
                            PublicPort = c.PublicPort,
                            ServiceType = c.ServiceType
                        }).ToList()
                }).ToList();

            var nfsServerState = new NFS_SERVERSTATE()
            {
                ServerCount = (byte)nfsServers.Count,
                Servers = nfsServers
            };

            using (var nfsServerStateS = await CabalSerializer.Instance.SerializeAsync(nfsServerState, Opcode.NFY_SERVERSTATE))
            {
                ServiceStateHandler.Instance.StoreServiceState(nfsServerStateS.Buffer.ToArray());
            }

            await ServiceStateHandler.Instance.BroadcastServerStateAsync();
        }
    }
}
