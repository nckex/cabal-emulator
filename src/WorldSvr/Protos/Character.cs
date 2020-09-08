using BinarySerialization;
using static Share.Protosdef;

namespace WorldSvr.Protos
 {
    static class Character
     {
        public class C2S_INITIALIZED : C2S_HEADER
         {
            [FieldOrder(0)] public int CharacterIdx { get; set; }
        }

        public class S2C_INITIALIZED : S2C_UNCOMPRESSED_HEADER
         {
            [FieldOrder(0)] [FieldLength(58)] public byte U0 { get; set; }
            [FieldOrder(1)] public byte U1 { get; set; }

            [FieldOrder(2)] public byte GroupIdx { get; set; }
            [FieldOrder(3)] public byte ServerIdx { get; set; }
            
            [FieldOrder(4)] [FieldLength(24)] public byte U2 { get; set; }

            [FieldOrder(5)] public ushort MaxPlayers { get; set; }
            
            [FieldOrder(6)] [FieldLength(4)] public byte[] IP { get; set; }
            [FieldOrder(7)] public ushort Port { get; set; }

            [FieldOrder(8)] public int U3 { get; set; }
            [FieldOrder(9)] public int U4 { get; set; }
            [FieldOrder(10)] public short U5 { get; set; }
            [FieldOrder(11)] public short U6 { get; set; }

            [FieldOrder(12)] public int WorldIdx { get; set; }
            [FieldOrder(13)] public int U7 { get; set; }
            [FieldOrder(14)] public short PosX { get; set; }
            [FieldOrder(15)] public short PosY { get; set; }

            [FieldOrder(16)] public long EXP { get; set; }
            [FieldOrder(17)] public long Alz { get; set; }
            [FieldOrder(18)] public long Wexp { get; set; }

            [FieldOrder(19)] public int LEV { get; set; }
            [FieldOrder(20)] public int U8 { get; set; }
            [FieldOrder(21)] public int STR { get; set; }
            [FieldOrder(22)] public int DEX { get; set; }
            [FieldOrder(23)] public int INT { get; set; }
            [FieldOrder(24)] public int PNT { get; set; }

            [FieldOrder(25)] public byte SwordRank { get; set; }
            [FieldOrder(26)] public byte MagicRank { get; set; }

            [FieldOrder(27)] public int U9 { get; set; }
            [FieldOrder(28)] public short U10 { get; set; }

            [FieldOrder(29)] public uint MaxHP { get; set; }
            [FieldOrder(30)] public uint CurHP { get; set; }
            [FieldOrder(31)] public uint MaxMP { get; set; }
            [FieldOrder(32)] public uint CurMP { get; set; }
            [FieldOrder(33)] public ushort MaxSP { get; set; }
            [FieldOrder(34)] public ushort CurSP { get; set; }

            [FieldOrder(35)] public int MaxRage { get; set; }

            [FieldOrder(36)] public int DP { get; set; }

            [FieldOrder(37)] public ushort U11 { get; set; }
            [FieldOrder(38)] public ushort U12 { get; set; }
            [FieldOrder(39)] public uint U13 { get; set; }

            [FieldOrder(40)] public uint SkillExpBars { get; set; }
            [FieldOrder(41)] public uint SkillExp { get; set; }
            [FieldOrder(42)] public uint SkillPoints { get; set; }

            [FieldOrder(43)] public ushort U14 { get; set; }

            [FieldOrder(44)] public int HonorPoints { get; set; }

            [FieldLength(28)]
            [FieldOrder(45)] public byte U15 { get; set; }

            [FieldOrder(46)] [FieldLength(4)] public byte[] IP1 { get; set; }
            [FieldOrder(47)] public ushort PORT1 { get; set; }

            [FieldOrder(48)] [FieldLength(4)] public byte[] IP2 { get; set; }
            [FieldOrder(49)] public ushort PORT2 { get; set; }
            
            [FieldOrder(50)] [FieldLength(4)] public byte[] IP3 { get; set; }
            [FieldOrder(51)] public ushort PORT3 { get; set; }
            [FieldOrder(52)] public ushort PORT4 { get; set; }

            [FieldLength(5)]
            [FieldOrder(53)] public byte U16 { get; set; }

            [FieldOrder(54)] public int U17 { get; set; }
            [FieldOrder(55)] public int U18 { get; set; }

            [FieldOrder(56)] public int Style { get; set; }

            [FieldOrder(57)] [FieldLength(244)] public byte U19 { get; set; }

            [FieldOrder(58)] public ushort EquipCount { get; set; }
            [FieldOrder(59)] public ushort InventoryCount { get; set; }
            [FieldOrder(61)] public ushort U20 { get; set; }
            [FieldOrder(62)] public ushort SkillCount { get; set; }
            [FieldOrder(63)] public ushort QuickSlotCount { get; set; }
            
            [FieldOrder(64)] [FieldLength(7944)] public byte U21 { get; set; }

            [Ignore] public int NameLength { get; set; }
            [FieldOrder(65)] public byte NameLengthNullByteTerminated { get; set; }
            [FieldOrder(66)] [FieldLength(nameof(NameLength))] public string Name { get; set; }

            [Ignore] public int EquipmentDataLen { get; set; }
            [FieldOrder(67)] [FieldLength(nameof(EquipmentDataLen))] public byte[] EquipmentData { get; set; }

            [Ignore] public int InventoryDataLen { get; set; }
            [FieldOrder(68)] [FieldLength(nameof(InventoryDataLen))] public byte[] InventoryData { get; set; }
        }
    }
}
