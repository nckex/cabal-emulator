using BinarySerialization;

namespace Share.Protos
{
    public static class Context
    {
        public class IPS_USERCONTEXT_DATA
        {
            [FieldOrder(0)] public int UserNum { get; set; }
            [FieldOrder(1)] [FieldLength(32)] public string AuthKey { get; set; }
            [FieldOrder(2)] public bool UseACSUB { get; set; }
            [FieldOrder(3)] public bool UseWHSUB { get; set; }
            [FieldOrder(4)] public bool UseEQSUB { get; set; }
            [FieldOrder(5)] public bool IsWHLOCK { get; set; }
            [FieldOrder(6)] public bool IsEQLOCK { get; set; }
            [FieldOrder(7)] public int ServiceType { get; set; }
            [FieldOrder(8)] public int ServiceKind { get; set; }
            [FieldOrder(9)] public int ExpirationDate { get; set; }
            [FieldOrder(10)] public int ExtendedCharCreation { get; set; }
        }
    }
}
