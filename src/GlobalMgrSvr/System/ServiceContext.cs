using HPT.Network;
using System.Threading;

namespace GlobalMgrSvr.System
{
    class ServiceContext
    {
        public ITcpSession Session { get; }

        public byte GroupIdx { get; private set; }
        public byte ServerIdx { get; private set; }
        public byte[] IP { get; private set; }
        public ushort Port { get; private set; }
        public ushort MaxConnections { get; private set; }
        public ushort OnlineCount => (ushort)_onlineCounter;
        public long Type { get; private set; }

        private int _onlineCounter;

        public ServiceContext(ITcpSession session)
        {
            Session = session;
        }

        public void IncrementOnlineCount()
        {
            Interlocked.Increment(ref _onlineCounter);
        }

        public void DecrementOnlineCount()
        {
            Interlocked.Decrement(ref _onlineCounter);
        }

        public void UpdateServiceInfos(byte groupIdx, byte serverIdx, byte[] ip, ushort port, ushort maxConnections, long type)
        {
            GroupIdx = groupIdx;
            ServerIdx = serverIdx;
            IP = ip;
            Port = port;
            MaxConnections = maxConnections;
            Type = type;
        }
    }
}
