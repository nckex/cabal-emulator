using BinarySerialization;
using static Share.Protosdef;
using static Share.Protos.Context;

namespace Share.Protos
{
    public static class Connect
    {
        public class C2S_VERIFYLINKS : C2S_HEADER
        {
            [FieldOrder(0)] public uint SessionTime { get; set; }
            [FieldOrder(1)] public ushort SessionId { get; set; }
            [FieldOrder(2)] public byte ServerIdx { get; set; }
            [FieldOrder(3)] public byte GroupIdx { get; set; }
            [FieldOrder(4)] public int NormalClientMagicKey { get; set; }
        }

        public class S2C_VERIFYLINKS : S2C_HEADER
        {
            [FieldOrder(0)] public byte ServerIdx { get; set; }
            [FieldOrder(1)] public byte GroupIdx { get; set; }
            [FieldOrder(2)] public bool IsLinked { get; set; }
        }

        public class IPS_LINK : IPS_HEADER
        {
            [FieldOrder(0)] public IPS_SERVICE FromService { get; set; }
            [FieldOrder(1)] public IPS_SESSION FromSession { get; set; }
            [FieldOrder(2)] public IPS_SERVICE ToService { get; set; }
            [FieldOrder(3)] public IPS_SESSION ToSession { get; set; }
            [FieldOrder(4)] public IPS_USERCONTEXT_DATA UserContextData { get; set; }
        }

        public class IPS_RESULT_LINK : IPS_HEADER
        {
            [FieldOrder(0)] public IPS_SERVICE ToService { get; set; }
            [FieldOrder(1)] public IPS_SESSION ToSession { get; set; }
            [FieldOrder(2)] public IPS_SERVICE FromService { get; set; }
            [FieldOrder(3)] public IPS_SESSION FromSession { get; set; }
            [FieldOrder(4)] public int UserNum { get; set; }
            [FieldOrder(5)] public bool IsLinked { get; set; }
        }

        public class IPS_FDISCONNECT : IPS_HEADER
        {
            [FieldOrder(0)] public IPS_SESSION ToSession { get; set; }
            [FieldOrder(1)] public int UserNum { get; set; }
        }

        public class IPS_RESULT_FDISCONNECT : IPS_HEADER
        {
            [FieldOrder(0)] public IPS_SERVICE FromService { get; set; }
            [FieldOrder(1)] public int UserNum { get; set; }
            [FieldOrder(2)] public bool SessionFound { get; set; }
        }
    }
}
