using BinarySerialization;
using static Share.Protosdef;

namespace Share.Protos
{
    public static class Character
    {
        public class IPS_GETMYCHARACTER : IPS_HEADER
        {
            [FieldOrder(0)] public IPS_SESSION FromSession { get; set; }
            [FieldOrder(1)] public int CharacterIdx { get; set; }
        }

        public class IPS_RESULT_GETMYCHARACTER : IPS_HEADER
        {
            [FieldOrder(0)] public IPS_SESSION ToSession { get; set; }

            [FieldOrder(1)] public int CharacterIdx { get; set; }
            [FieldOrder(2)] public int NameLen { get; set; }
            [FieldOrder(3)] [FieldLength(nameof(NameLen))] public string Name { get; set; }
            [FieldOrder(4)] public int Style { get; set; }
            [FieldOrder(5)] public int LEV { get; set; }
            [FieldOrder(6)] public long EXP { get; set; }
            [FieldOrder(7)] public short STR { get; set; }
            [FieldOrder(8)] public short DEX { get; set; }
            [FieldOrder(9)] public short INT { get; set; }
            [FieldOrder(10)] public short PNT { get; set; }


            [FieldOrder(11)] public short WorldIdx { get; set; }
            [FieldOrder(12)] public int Position { get; set; }

            [FieldOrder(13)] public int EquipmentDataLen { get; set; }
            [FieldOrder(14)] [FieldLength(nameof(EquipmentDataLen))] public byte[] EquipmentData { get; set; }

            [FieldOrder(15)] public int InventoryDataLen { get; set; }
            [FieldOrder(16)] [FieldLength(nameof(InventoryDataLen))] public byte[] InventoryData { get; set; }
        }
    }
}
