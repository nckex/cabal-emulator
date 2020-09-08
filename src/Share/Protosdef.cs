using BinarySerialization;

namespace Share
{
    public static class Protosdef
    {
        public class C2S_HEADER
        {
            [FieldOrder(0)] public ushort MagicCode { get; set; }
            [FieldOrder(1)] public ushort TotalSize { get; set; }
            [FieldOrder(2)] public uint Checksum { get; set; }
            [FieldOrder(3)] public ushort Opcode { get; set; }
        }

        public class S2C_HEADER
        {
            [FieldOrder(0)] public ushort MagicCode { get; set; }
            [FieldOrder(1)] public ushort TotalSize { get; set; }
            [FieldOrder(3)] public ushort Opcode { get; set; }
        }

        public class S2C_UNCOMPRESSED_HEADER
        {
            [FieldOrder(0)] public ushort MagicCode { get; set; }
            [FieldOrder(1)] public int TotalSize { get; set; }
            [FieldOrder(3)] public ushort Opcode { get; set; }
        }

        public class IPS_HEADER
        {
            [FieldOrder(0)] public ushort TotalSize { get; set; }
            [FieldOrder(1)] public ushort Opcode { get; set; }
        }

        public class IPS_SERVICE
        {
            [FieldOrder(0)] public byte GroupIdx { get; set; }
            [FieldOrder(1)] public byte ServerIdx { get; set; }
        }

        public class IPS_SESSION
        {
            [FieldOrder(0)] public ushort SessionId { get; set; }
            [FieldOrder(1)] public uint SessionTime { get; set; }
        }
    }
}
