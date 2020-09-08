using BinarySerialization;
using static Share.Protosdef;

namespace WorldSvr.Protos
{
    static class Charge
    {
        public class S2C_CHARGEINFO : S2C_HEADER
        {
            [FieldOrder(0)]
            public int ServiceType { get; set; }

            [FieldOrder(1)]
            public int ServiceKind { get; set; }

            [FieldOrder(2)]
            public int ExpireDate { get; set; }
        }
    }
}
