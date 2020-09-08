using BinarySerialization;
using static Share.Protodef;
using static Share.Protosdef;

namespace Share.Protos
{
    public static class Subpassword
    {
        public class IPS_SUBPASSWD_ASK : IPS_HEADER
        {
            [FieldOrder(0)] public IPS_SESSION FromSession { get; set; }
            [FieldOrder(1)] public int UserNum { get; set; }
            [FieldOrder(2)] public uint IpAddress { get; set; }        
        }

        public class IPS_RESULT_SUBPASSWD_ASK : IPS_HEADER
        {
            [FieldOrder(0)] public IPS_SESSION ToSession { get; set; }
            [FieldOrder(1)] public bool IsRequired { get; set; }        
        }

        public class IPS_SUBPASSWD_SET : IPS_HEADER
        {
            [FieldOrder(0)] public IPS_SESSION FromSession { get; set; }
            [FieldOrder(1)] public int UserNum { get; set; }
            [FieldOrder(2)] public SubpasswordType Type { get; set; }
            [FieldOrder(3)] public SubpasswordMode Mode { get; set; }
            [FieldOrder(4)]
            [FieldLength(MAX_SUBPASSWD_LENGHT)] public string Password { get; set; }
            [FieldOrder(5)] public int Question { get; set; }
            [FieldOrder(6)]
            [FieldLength(MAX_SUBPASSWD_ANSWER_LEN)] public string Answer { get; set; }        
        }

        public class IPS_RESULT_SUBPASSWD_SET : IPS_HEADER
        {
            [FieldOrder(0)] public IPS_SESSION ToSession { get; set; }
            [FieldOrder(1)] public bool Success { get; set; }
            [FieldOrder(2)] public SubpasswordType Type { get; set; }
            [FieldOrder(3)] public SubpasswordMode Mode { get; set; }        
        }

        public class IPS_SUBPASSWD_AUTH : IPS_HEADER
        {
            [FieldOrder(0)] public IPS_SESSION FromSession { get; set; }
            [FieldOrder(1)] public ushort FromOpcode { get; set; }
            [FieldOrder(2)] public int UserNum { get; set; }
            [FieldOrder(3)] public SubpasswordType Type { get; set; }
            [FieldOrder(4)]
            [FieldLength(MAX_SUBPASSWD_LENGHT)] public string Password { get; set; }        
        }

        public class IPS_RESULT_SUBPASSWD_AUTH : IPS_HEADER
        {
            [FieldOrder(0)] public IPS_SESSION ToSession { get; set; }
            [FieldOrder(1)] public ushort ToOpcode { get; set; }
            [FieldOrder(2)] public bool Success { get; set; }
            [FieldOrder(3)] public SubpasswordType Type { get; set; }
            [FieldOrder(4)] public int TryNum { get; set; }
            [FieldOrder(5)] public bool AccountBlocked { get; set; }        
        }

        public class IPS_SUBPASSWD_AUTH_REMEMBER : IPS_SUBPASSWD_AUTH
        {
            [FieldOrder(0)] public int RememberHours { get; set; }
            [FieldOrder(1)] public uint IPAddress32 { get; set; }        
        }

        public class IPS_SUBPASSWD_GET_QA : IPS_HEADER
        {
            [FieldOrder(0)] public IPS_SESSION FromSession { get; set; }
            [FieldOrder(1)] public int UserNum { get; set; }
            [FieldOrder(2)] public SubpasswordType Type { get; set; }        
        }

        public class IPS_RESULT_SUBPASSWD_GET_QA : IPS_HEADER
        {
            [FieldOrder(0)] public IPS_SESSION ToSession { get; set; }
            [FieldOrder(1)] public SubpasswordType Type { get; set; }
            [FieldOrder(2)] public bool Success { get; set; }
            [FieldOrder(3)] public int QuestionId { get; set; }        
        }

        public class IPS_SUBPASSWD_ANSWER_QA : IPS_HEADER
        {
            [FieldOrder(0)] public IPS_SESSION FromSession { get; set; }
            [FieldOrder(1)] public int UserNum { get; set; }
            [FieldOrder(2)] public SubpasswordType Type { get; set; }
            [FieldOrder(3)] public int QuestionId { get; set; }
            [FieldOrder(4)]
            [FieldLength(MAX_SUBPASSWD_ANSWER_LEN)] public string Answer { get; set; }        
        }

        public class IPS_RESULT_SUBPASSWD_ANSWER_QA : IPS_HEADER
        {
            [FieldOrder(0)] public IPS_SESSION ToSession { get; set; }
            [FieldOrder(1)] public SubpasswordType Type { get; set; }
            [FieldOrder(2)] public bool Success { get; set; }
            [FieldOrder(4)] public byte TryNum { get; set; }
            [FieldOrder(5)] public bool AccountBlocked { get; set; }        
        }

        public class IPS_SUBPASSWD_DEL : IPS_HEADER
        {
            [FieldOrder(0)] public IPS_SESSION FromSession { get; set; }
            [FieldOrder(1)] public int UserNum { get; set; }
            [FieldOrder(2)] public SubpasswordType Type { get; set; }        
        }

        public class IPS_RESULT_SUBPASSWD_DEL : IPS_HEADER
        {
            [FieldOrder(0)] public IPS_SESSION ToSession { get; set; }
            [FieldOrder(1)] public bool Success { get; set; }
            [FieldOrder(2)] public SubpasswordType Type { get; set; }       
        }

        public class IPS_SUBPASSWD_CHANGE_QA : IPS_HEADER
        {
            [FieldOrder(0)] public IPS_SESSION FromSession { get; set; }
            [FieldOrder(1)] public int UserNum { get; set; }
            [FieldOrder(2)] public SubpasswordType Type { get; set; }
            [FieldOrder(3)] public int QuestionId { get; set; }
            [FieldOrder(4)]
            [FieldLength(MAX_SUBPASSWD_ANSWER_LEN)] public string Answer { get; set; }      
        }

        public class IPS_RESULT_SUBPASSWD_CHANGE_QA : IPS_HEADER
        {
            [FieldOrder(0)] public IPS_SESSION ToSession { get; set; }
            [FieldOrder(1)] public SubpasswordType Type { get; set; }
            [FieldOrder(2)] public bool Success { get; set; }     
        }
    }
}
