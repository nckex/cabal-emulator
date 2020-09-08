namespace Share.Encryption
{
    public class CabalEncryptable : ICabalEncryptable
    {
        public uint HeaderXor { get; set; }
        public uint Step { get; set; }
        public uint Mul { get; set; }

        public CabalEncryptable()
        {
            HeaderXor = 0xB43CC06E;
            Step = 0;
            Mul = 1;
        }
    }
}
