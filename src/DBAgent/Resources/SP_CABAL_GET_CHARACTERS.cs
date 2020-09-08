namespace DBAgent.Resources
{
    class SP_CABAL_GET_CHARACTERS
    {
        public int CharacterIdx { get; protected set; }
        public string Name { get; protected set; }
        public ushort LEV { get; protected set; }
        public int Style { get; protected set; }
        public byte WorldIdx { get; protected set; }
        public int Position { get; protected set; }
        public int Rank { get; protected set; }
        public int CreationDate { get; protected set; }
        public byte[] EquipmentData { get; protected set; }
    }
}
