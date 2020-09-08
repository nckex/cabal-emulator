namespace Share
{
    public static class Opcode
    {
        public const ushort

            // Service
            IPC_ADDSERVICE = 100,
            IPC_RESULT_ADDSERVICE = 101,
            IPC_SERVICESTATE = 102,
            IPC_RESULT_SERVICESTATE = 103,
            // End Service

            // Auth
            IPC_AUTH = 200,
            IPC_RESULT_AUTH = 201,
            IPC_LOGINSTATE = 202,
            IPC_RESULT_LOGINSTATE = 203,
            IPC_REQ_FDISCONNECT = 204,
            IPC_RESULT_REQ_FDISCONNECT = 205,
            IPC_FDISCONNECT = 206,
            IPC_RESULT_FDISCONNECT = 207,
            // End Auth

            // Link
            IPC_LINK = 300,
            IPC_RESULT_LINK = 301,
            // End Link

            // Char Lobby
            IPC_GETCHARS = 400,
            IPC_RESULT_GETCHARS = 401,
            IPC_NEWCHAR = 402,
            IPC_RESULT_NEWCHAR = 403,
            IPC_CHAR_SLOTORDER = 404,
            IPC_RESULT_CHAR_SLOTORDER = 405,
            IPC_CHECKPASSWD = 406,
            IPC_RESULT_CHECKPASSWD = 407,
            IPC_DELCHAR = 408,
            IPC_RESULT_DELCHAR = 409,
            // End Char Lobby

            // Subpassword
            IPC_SUBPASSWD_ASK = 500,
            IPC_RESULT_SUBPASSWD_ASK = 501,
            IPC_SUBPASSWD_SET = 502,
            IPC_RESULT_SUBPASSWD_SET = 503,
            IPC_SUBPASSWD_AUTH = 504,
            IPC_RESULT_SUBPASSWD_AUTH = 505,
            IPC_SUBPASSWD_AUTH_REMEMBER = 506,
            IPC_RESULT_SUBPASSWD_AUTH_REMEMBER = 507,
            IPC_SUBPASSWD_GET_QA = 508,
            IPC_RESULT_SUBPASSWD_GET_QA = 509,
            IPC_SUBPASSWD_ANSWER_QA = 510,
            IPC_RESULT_SUBPASSWD_ANSWER_QA = 511,
            IPC_SUBPASSWD_DEL = 512,
            IPC_RESULT_SUBPASSWD_DEL = 513,
            IPC_SUBPASSWD_CHANGE_QA = 514,
            IPC_RESULT_SUBPASSWD_CHANGE_QA = 515,
            // End Subpassword

            // Character
            IPC_GETMYCHARACTER = 600,
            IPC_RESULT_GETMYCHARACTER = 601,
            // End Character



















            _UNUSED = 0;
    }
}
