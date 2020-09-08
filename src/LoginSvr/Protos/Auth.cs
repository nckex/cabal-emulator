using BinarySerialization;
using Share.Encryption;
using static Share.Protodef;
using static Share.Protosdef;

namespace LoginSvr.Protos
{
    static class Auth
    {
        public class C2S_PRE_AUTHENTICATE : C2S_HEADER
        {
            [FieldOrder(0)]
            [FieldLength(35)]
            public string Username { get; set; }
        }

        public class S2C_PRE_AUTHENTICATE : S2C_HEADER
        {
            [FieldOrder(0)]
            [FieldLength(4108)]
            public byte U0 { get; set; }
        }

        public class S2C_RSA_PUBLIC_KEY : S2C_HEADER
        {
            [FieldOrder(0)]
            public bool Status;

            [FieldOrder(1)]
            public ushort KeyLength;

            [FieldOrder(2)]
            [FieldLength(nameof(KeyLength))]
            public byte[] PublicKey;
        }

        public class S2C_PREAUTH_STATUS : S2C_HEADER
        {
            [FieldOrder(0)]
            public int U0 { get; set; }
        }

        public class NFS_U2005 : S2C_HEADER
        {
            [FieldOrder(0)]
            public int U0 { get; set; }

            [FieldOrder(1)]
            [FieldLength(5)]
            public byte U1 { get; set; }
        }

        public class C2S_AUTHENTICATE : C2S_HEADER
        {
            [FieldOrder(0)]
            public int Unk0 { get; set; }

            [FieldOrder(1)]
            [FieldLength(RSA.KEY_SIZE/8)]
            public byte[] EncryptedData { get; set; }
        }

        public class S2C_AUTHENTICATE_HEADER
        {
            [FieldOrder(0)]
            public bool KeepAlive { get; set; }

            [FieldOrder(1)]
            public int U0 { get; set; }

            [FieldOrder(2)]
            public int SubMessageType { get; set; }

            [FieldOrder(3)]
            [FieldLength(4)]
            public AuthResult AuthResult { get; set; }
        }

        public class S2C_AUTHENTICATE : S2C_HEADER
        {
            [FieldOrder(0)]
            public S2C_AUTHENTICATE_HEADER AuthenticateHeader { get; set; }

            [FieldOrder(1)]
            public int UserNum { get; set; }

            [FieldOrder(2)]
            public int U0 { get; set; }

            [FieldOrder(3)]
            [FieldLength(6)]
            public byte U1 { get; set; }

            [FieldOrder(4)]
            public int ServiceKind { get; set; }

            [FieldOrder(5)]
            public int ExpirationDate { get; set; }

            [FieldOrder(6)]
            [FieldLength(5)]
            public byte U2 { get; set; }

            [FieldOrder(7)]
            public long U3 { get; set; }

            [FieldOrder(8)]
            [FieldLength(32)]
            public string AuthKey { get; set; }

            [FieldOrder(9)]
            [FieldLength(259)]
            public byte Unk4 { get; set; }
        }

        public class NFS_LOGINTIMER : S2C_HEADER
        {
            [FieldOrder(0)]
            public int Milliseconds { get; set; }
        }

        public class NFS_SYSTEMMESSG : S2C_HEADER
        {
            [FieldOrder(0)]
            public SystemMessage SystemMessage { get; set; }

            [FieldOrder(1)]
            public short U0 { get; set; }
        }

        public class NFS_URLTOCLIENT : S2C_HEADER
        {
            [FieldOrder(0)]
            public short U0 { get; set; }

            [FieldOrder(1)]
            public short U1 { get; set; }

            [FieldOrder(2)]
            public int URL1Len { get; set; }

            [FieldOrder(3)]
            [FieldLength(nameof(URL1Len))]
            public string URL1 { get; set; }

            [FieldOrder(4)]
            public int URL2Len { get; set; }

            [FieldOrder(5)]
            [FieldLength(nameof(URL2Len))]
            public string URL2 { get; set; }

            [FieldOrder(6)]
            public int URL3Len { get; set; }

            [FieldOrder(7)]
            [FieldLength(nameof(URL3Len))]
            public string URL3 { get; set; }

            [FieldOrder(8)]
            public int U2 { get; set; }

            [FieldOrder(9)]
            public int U3 { get; set; }

            [FieldOrder(10)]
            public short U4 { get; set; }

            [FieldOrder(11)]
            public int URL4Len { get; set; }

            [FieldOrder(12)]
            [FieldLength(nameof(URL4Len))]
            public string URL4 { get; set; }

            [FieldOrder(13)]
            public int URL5Len { get; set; }

            [FieldOrder(14)]
            [FieldLength(nameof(URL5Len))]
            public string URL5 { get; set; }
        }

        public class S2C_FDISCONNECT : S2C_HEADER
        {
            [FieldOrder(0)]
            public bool Success { get; set; }
        }
    }
}
