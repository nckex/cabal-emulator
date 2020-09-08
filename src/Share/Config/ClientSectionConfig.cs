namespace Share.Config
{
    public class ClientSectionConfig
    {
        public string IP { get; set; }
        public ushort Port { get; set; }
        public bool NoDelay { get; set; }
        public int Retry { get; set; }
        public int HeaderSize { get; set; }
        public bool AbortOnDisconnect { get; set; }
    }
}
