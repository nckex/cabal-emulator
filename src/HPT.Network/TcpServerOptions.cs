namespace HPT.Network
{
    public class TcpServerOptions
    {
        public string Address { get; }
        public ushort Port { get; }
        public bool NoDelay { get; }
        public int Backlog { get; }
        public int HeaderSize { get; }

        public TcpServerOptions(string address, ushort port, bool noDelay = true, int backlog = 100, int headerSize = 2)
        {
            Address = address;
            Port = port;
            NoDelay = noDelay;
            Backlog = backlog;
            HeaderSize = headerSize;
        }
    }
}
