using BinarySerialization;
using static Share.Protodef;
using static Share.Protosdef;

namespace WorldSvr.Protos
{
    static class Subpassword
    {
        public class S2C_SUBPASSWORD_ASK : S2C_HEADER
        {
            [FieldOrder(0)]
            [FieldLength(4)]
            public bool IsRequired { get; set; }
        }

        public class C2S_SUBPASSWORD_SET : C2S_HEADER
        {
            [FieldOrder(0)]
            [FieldLength(MAX_SUBPASSWD_LENGHT)]
            public string Password { get; set; }

            [FieldOrder(1)]
            [FieldLength(4)]
            public SubpasswordType Type { get; set; }

            [FieldOrder(2)]
            public int Question { get; set; }

            [FieldOrder(3)]
            [FieldLength(MAX_SUBPASSWD_ANSWER_LEN)]
            public string Answer { get; set; }

            [FieldOrder(4)]
            [FieldLength(112)]
            public byte U0 { get; set; }

            [FieldOrder(5)]
            [FieldLength(4)]
            public SubpasswordMode Mode { get; set; }
        }

        public class S2C_SUBPASSWORD_SET : S2C_HEADER
        {
            [FieldOrder(0)]
            [FieldLength(4)]
            public bool Success { get; set; }

            [FieldOrder(1)]
            [FieldLength(4)]
            public SubpasswordMode Mode { get; set; }

            [FieldOrder(2)]
            [FieldLength(4)]
            public SubpasswordType Type { get; set; }

            [FieldOrder(3)]
            public int U0 { get; set; }
        }

        public class C2S_SUBPASSWORD_AUTH_REMEMBER : C2S_HEADER
        {
            [FieldOrder(0)]
            [FieldLength(MAX_SUBPASSWD_LENGHT)]
            public string Password { get; set; }

            [FieldOrder(1)]
            [FieldLength(4)]
            public SubpasswordType Type { get; set; }

            [FieldOrder(2)]
            public int RememberHours { get; set; }

            [FieldOrder(3)]
            public byte U0 { get; set; }
        }

        public class S2C_SUBPASSWORD_AUTH_REMEMBER : S2C_HEADER
        {
            [FieldOrder(0)]
            [FieldLength(4)]
            public bool Success { get; set; }

            [FieldOrder(1)]
            public int TryNum { get; set; }

            [FieldOrder(2)]
            public byte U0 { get; set; }

            [FieldOrder(3)]
            [FieldLength(4)]
            public SubpasswordType Type { get; set; }
        }

        public class C2S_SUBPASSWORD_CHANGE_QA_AUTH : C2S_HEADER
        {
            [FieldOrder(0)]
            [FieldLength(4)]
            public SubpasswordType Type { get; set; }

            [FieldOrder(2)]
            [FieldLength(MAX_SUBPASSWD_LENGHT)]
            public string Password { get; set; }
        }

        public class C2S_SUBPASSWORD_DEL_AUTH : C2S_HEADER
        {
            [FieldOrder(0)]
            [FieldLength(4)]
            public SubpasswordType Type { get; set; }

            [FieldOrder(1)]
            [FieldLength(65)]
            public byte U0 { get; set; }

            [FieldOrder(2)]
            [FieldLength(MAX_SUBPASSWD_LENGHT)]
            public string Password { get; set; }
        }

        public class S2C_SUBPASSWORD_AUTH : S2C_HEADER
        {
            [FieldOrder(0)]
            [FieldLength(4)]
            public bool Success { get; set; }

            [FieldOrder(1)]
            public int TryNum { get; set; }

            [FieldOrder(2)]
            [FieldLength(4)]
            public SubpasswordType Type { get; set; }
        }

        public class C2S_SUBPASSWORD_GET_QA : C2S_HEADER
        {
            [FieldOrder(0)]
            [FieldLength(4)]
            public SubpasswordType Type { get; set; }
        }

        public class S2C_SUBPASSWORD_GET_QA : S2C_HEADER
        {
            [FieldOrder(0)]
            public int QuestionId1 { get; set; }

            [FieldOrder(1)]
            public int QuestionId2 { get; set; }

            [FieldOrder(2)]
            [FieldLength(4)]
            public SubpasswordType Type { get; set; }
        }

        public class C2S_SUBPASSWORD_ANSWER_QA : C2S_HEADER
        {
            [FieldOrder(0)]
            [FieldLength(4)]
            public SubpasswordType Type { get; set; }

            [FieldOrder(1)]
            public int QuestionId { get; set; }

            [FieldOrder(2)]
            [FieldLength(MAX_SUBPASSWD_ANSWER_LEN)]
            public string Answer { get; set; }
        }

        public class S2C_SUBPASSWORD_ANSWER_QA : S2C_HEADER
        {
            [FieldOrder(0)]
            [FieldLength(4)]
            public bool Success { get; set; }

            [FieldOrder(1)]
            public byte TryNum { get; set; }

            [FieldOrder(2)]
            [FieldLength(4)]
            public SubpasswordType Type { get; set; }
        }

        public class C2S_SUBPASSWORD_DEL : C2S_HEADER
        {
            [FieldOrder(0)]
            [FieldLength(4)]
            public SubpasswordType Type { get; set; }
        }

        public class S2C_SUBPASSWORD_DEL : S2C_HEADER
        {
            [FieldOrder(0)]
            [FieldLength(4)]
            public bool Success { get; set; }

            [FieldOrder(1)]
            [FieldLength(4)]
            public SubpasswordType Type { get; set; }
        }

        public class C2S_SUBPASSWORD_CHANGE_QA : C2S_HEADER
        {
            [FieldOrder(0)]
            [FieldLength(4)]
            public SubpasswordType Type { get; set; }

            [FieldOrder(1)]
            public int QuestionId { get; set; }

            [FieldOrder(2)]
            [FieldLength(MAX_SUBPASSWD_ANSWER_LEN)]
            public string Answer { get; set; }
        }

        public class S2C_SUBPASSWORD_CHANGE_QA : S2C_HEADER
        {
            [FieldOrder(0)]
            [FieldLength(4)]
            public bool Success { get; set; }

            [FieldOrder(1)]
            [FieldLength(4)]
            public SubpasswordType Type { get; set; }
        }
    }
}
