namespace DBAgent.Resources
{
    class SP_CABAL_GET_CHARACTER
    {
        public int CharacterIdx { get; protected set; }
        public string Name { get; protected set; }
        public int Style { get; protected set; }
        public int LEV { get; protected set; }
        public long EXP { get; protected set; }
        public short STR { get; protected set; }
        public short DEX { get; protected set; }
        public short INT { get; protected set; }
        public short PNT { get; protected set; }


        public short WorldIdx { get; protected set; }
        public int Position { get; protected set; }

        public byte[] EquipmentData { get; protected set; }
        public byte[] InventoryData { get; protected set; }
    }
}
