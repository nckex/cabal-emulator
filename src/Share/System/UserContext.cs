namespace Share.System
{
    public class UserContext
    {
        public int UserNum { get; set; }
        public string AuthKey { get; set; }
        public bool UseACSUB { get; set; }
        public bool UseWHSUB { get; set; }
        public bool UseEQSUB { get; set; }
        public bool IsWHLOCK { get; set; }
        public bool IsEQLOCK { get; set; }
        public int ServiceType { get; set; }
        public int ServiceKind { get; set; }
        public int ExpirationDate { get; set; }
        public int ExtendedCharCreation { get; set; }

        public bool ShouldSoftDisconnect { get; set; }
    }
}
