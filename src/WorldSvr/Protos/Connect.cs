using BinarySerialization;
using static Share.Protosdef;

namespace WorldSvr.Protos
{
    static class Connect
    {
        public class C2S_CONNECT2SERV : C2S_HEADER
        {
            [FieldOrder(0)]
            public byte ServerIdx { get; set; }

            [FieldOrder(1)]
            public byte GroupIdx { get; set; }

            [FieldOrder(2)]
            public byte Reserved0 { get; set; }

            [FieldOrder(3)]
            public byte Reserved1 { get; set; }
        }

        public class S2C_CONNECT2SERV : S2C_HEADER
        {
            [FieldOrder(0)]
            public uint Recv2ndXorSeed { get; set; }

            [FieldOrder(1)]
            public uint AuthKey { get; set; }

            [FieldOrder(2)]
            public ushort Index { get; set; }

            [FieldOrder(3)]
            public ushort RecvXorKeyIdx { get; set; }
        }
    }
}
