using static Share.Protodef;

namespace GlobalMgrSvr.Resources
{
    class SP_CABAL_AUTH_ACCOUNT
    {
        public AuthResult AuthResult { get; protected set; }
        public int UserNum { get; protected set; }
        public string AuthKey { get; protected set; }
        public bool UseACSUB { get; protected set; }
        public bool UseWHSUB { get; protected set; }
        public bool UseEQSUB { get; protected set; }
        public bool IsWHLOCK { get; protected set; }
        public bool IsEQLOCK { get; protected set; }
        public int ServiceType { get; protected set; }
        public int ServiceKind { get; protected set; }
        public int ExpirationDate { get; protected set; }
        public int ExtendedCharCreation { get; protected set; }
    }
}
