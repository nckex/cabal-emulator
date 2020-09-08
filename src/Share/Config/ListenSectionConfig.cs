namespace Share.Config
{
    public class ListenSectionConfig
    {
        public string IP { get; set; }
        public ushort Port { get; set; }
        public bool NoDelay { get; set; }
        public int Backlog { get; set; }
        public int HeaderSize { get; set; }
    }
}
