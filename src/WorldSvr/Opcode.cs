namespace WorldSvr
{
    static class Opcode
    {
        public const ushort

            // Connect
            CSC_CONNECT2SERV = 140,
            CSC_VERIFYLINKS = 141,
            // End Connect

            // Server
            CSC_GETSVRTIME = 148,
            CSC_SERVERENV = 464,
            CSC_PING = 2558,
            // End Server

            // Nofity
            NFY_SYSTEMMESSG = 241,
            // End Notify

            // Charge
            CSC_CHARGEINFO = 324,
            // End Charge

            // CharLobby
            CSC_GETMYCHARTR = 133,
            CSC_NEWMYCHARTR = 134,
            CSC_DELMYCHARTR = 135,
            CSC_CHECKPASSWD = 800,
            CSC_CHARACTER_SLOTORDER = 2001,
            CSC_SPECIALCHAREVT = 2156,
            // End CharLobby

            // Subpassword
            CSC_SUBPASSWORD_SET = 1030,
            CSC_SUBPASSWORD_ASK = 1032,
            CSC_SUBPASSWORD_AUTH = 1034,
            CSC_SUBPASSWORD_GET_QA = 1036,
            CSC_SUBPASSWORD_ANSWER_QA = 1038,
            CSC_SUBPASSWORD_DEL_AUTH = 1040,
            CSC_SUBPASSWORD_DEL = 1042,
            CSC_SUBPASSWORD_CHANGE_QA_AUTH = 1044,
            CSC_SUBPASSWORD_CHANGE_QA = 1046,
            // End Subpassword

            // Character
            CSC_INITIALIZED = 142,
            // End Character









            _UNUSED = 0;
    }
}
