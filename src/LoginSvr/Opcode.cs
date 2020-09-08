namespace LoginSvr
{
    static class Opcode
    {
        public const ushort

            // Connect
            CSC_CONNECT2SERV = 101,
            CSC_VERIFYLINKS = 102,
            CSC_FDISCONNECT = 109,
            CSC_CHECKVERSION = 122,
            // End Connect

            // Auth
            CSC_PRE_AUTHENTICATE = 2002,
            CSC_RSA_PUBLIC_KEY = 2001,
            CSC_PREAUTH_STATUS = 103,
            CSC_AUTHENTICATE = 2006,
            NFY_LOGINTIMER = 2009,
            // End Auth

            NFY_SYSTEMMESSG = 120,
            NFY_SERVERSTATE = 121,
            NFY_URLTOCLIENT = 128,

            // Unknown
            NFY_U2005 = 2005,
            // End Unknown

            // Unused
            UNUSED_3383 = 3383,
            // End Unused
























            _UNUSED = 0;
    }
}
