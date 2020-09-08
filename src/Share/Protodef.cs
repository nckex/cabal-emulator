namespace Share
{
    public static class Protodef
    {
        public const int
            MAX_USERNAME_LENGHT = 65,
            MAX_PASSWORD_LENGHT = 65,
            MAX_SUBPASSWD_LENGHT = 11,
            MAX_SUBPASSWD_ANSWER_LEN = 16,
            MAX_ITEMBAG_NUM = 1024;

        public enum AuthResult : byte
        {
            Success = 0x20,
            Incorrect = 0x21,
            Already = 0x22,
            Outfsvc = 0x23,
            Credits = 0x24,
            BlockIp = 0x25,
            BlockId = 0x26,
            FreeId = 0x27,
            NetCafeOnly = 0x28,
            Inactive = 0x29,
            BlockSub = 0x31
        }

        public enum SystemMessage : byte
        {
            Normal = 0x00,
            LoginDuplicate = 0x01,
            ForceDisconnect = 0x02,
            Shutdown = 0x03,
            ShutdownWoNotice = 0x04,
            War_Cappela = 0x05,
            War_Procyon = 0x06,
            NewNormal = 0x09
        }

        public enum LoginStateMode : byte
        {
            Connect,
            Disconnect,
            SoftDisconnect
        }

        public enum CharResult : byte
        {
            SuccessWA = 0xA1,
            SuccessBL = 0xA2,
            SuccessWI = 0xA3,
            SuccessFA = 0xA4,
            SuccessFS = 0xA5,
            SuccessFB = 0xA6,
            SuccessGL = 0xA7,
            SuccessFG = 0xA8,
            DBError = 0x01,
            NamedUp = 0x03,
            BadWord = 0x04
        }

        public enum DelResult : byte
        {
            Success = 0xA1,
            GuildMaster = 0xB2
        }

        public enum SubpasswordMode : byte
        {
            Create,
            Update
        }

        public enum SubpasswordType : byte
        {
            Account = 0x01,
            Wharehouse = 0x02,
            Equipment = 0x03
        }
    }
}
