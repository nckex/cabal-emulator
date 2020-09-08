namespace HPT.Network
{
    public class TcpClientOptions
    {
        public string Address { get; }
        public ushort Port { get; }
        public bool NoDelay { get; }
        public int Retry { get; }
        public bool AbortOnDisconnect { get; }
        public int HeaderSize { get; }

        public TcpClientOptions(string address, ushort port, bool noDelay = true, int retry = 3, bool abortOnDisconnect = false, int headerSize = 2)
        {
            Address = address;
            Port = port;
            NoDelay = noDelay;
            Retry = retry;
            AbortOnDisconnect = abortOnDisconnect;
            HeaderSize = headerSize;
        }
    }
}
