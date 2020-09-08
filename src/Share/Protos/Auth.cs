using BinarySerialization;
using static Share.Protodef;
using static Share.Protosdef;
using static Share.Protos.Context;

namespace Share.Protos
{
    public static class Auth
    {
        public class IPS_AUTH : IPS_HEADER
        {
            [FieldOrder(0)] public IPS_SERVICE FromService { get; set; }
            [FieldOrder(1)] public IPS_SESSION FromSession { get; set; }
            [FieldOrder(2)] [FieldLength(MAX_USERNAME_LENGHT)] public string Username { get; set; }
            [FieldOrder(3)] [FieldLength(MAX_PASSWORD_LENGHT)] public string Password { get; set; }
            [FieldOrder(4)] [FieldLength(16)] public string IpAddress { get; set; }
        }

        public class IPS_RESULT_AUTH : IPS_HEADER
        {
            [FieldOrder(0)] public IPS_SERVICE ToService { get; set; }
            [FieldOrder(1)] public IPS_SESSION ToSession { get; set; }
            [FieldOrder(2)] public AuthResult AuthResult { get; set; }
            [FieldOrder(3)] public IPS_USERCONTEXT_DATA UserContextData { get; set; }
        }

        public class IPS_LOGINSTATE : IPS_HEADER
        {
            [FieldOrder(0)] public IPS_SERVICE FromService { get; set; }
            [FieldOrder(1)] public IPS_SESSION FromSession { get; set; }
            [FieldOrder(2)] public int UserNum { get; set; }
            [FieldOrder(3)] public LoginStateMode Mode { get; set; }
        }

        public class IPS_RESULT_LOGINSTATE : IPS_HEADER
        {
            [FieldOrder(0)] public IPS_SERVICE ToService { get; set; }
            [FieldOrder(1)] public IPS_SESSION ToSession { get; set; }
        }

        public class IPS_REQ_FDISCONNECT : IPS_HEADER
        {
            [FieldOrder(0)] public IPS_SESSION FromSession { get; set; }
            [FieldOrder(1)] public int UserNum { get; set; }
        }

        public class IPS_RESULT_REQ_FDISCONNECT : IPS_HEADER
        {
            [FieldOrder(0)] public IPS_SESSION ToSession { get; set; }
            [FieldOrder(1)] public bool Success { get; set; }
        }
    }
}
