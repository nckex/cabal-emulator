using System.Collections.Generic;
using BinarySerialization;
using static Share.Protosdef;

namespace Share.Protos
{
    public static class Services
    {
        public class IPS_ADDSERVICE : IPS_HEADER
        {
            [FieldOrder(0)] public IPS_SERVICE Service { get; set; }
            [FieldOrder(1)] [FieldLength(4)] public byte[] IP { get; set; }
            [FieldOrder(2)] public ushort Port { get; set; }
            [FieldOrder(3)] public ushort MaxConnections { get; set; }
            [FieldOrder(4)] public long ServiceType { get; set; }
        }

        public class IPS_RESULT_ADDSERVICE : IPS_HEADER
        {
            [FieldOrder(0)] public IPS_SERVICE Service { get; set; }
            [FieldOrder(1)] public bool Success { get; set; }
        }

        public class IPS_SERVICESTATE : IPS_HEADER
        {
            [FieldOrder(0)] public byte GroupIdx { get; set; }
            [FieldOrder(1)] public byte ServerIdx { get; set; }
        }

        public class IPS_RESULT_SERVICESTATE : IPS_HEADER
        {
            [FieldOrder(0)] public byte ServerCount { get; set; }
            [FieldOrder(1)] [FieldCount(nameof(ServerCount))] public List<IPS_RESULT_SERVICESTATE_SERVER> Servers { get; set; }
        }

        public class IPS_RESULT_SERVICESTATE_SERVER
        {
            [FieldOrder(0)] public byte GroupIdx { get; set; }
            [FieldOrder(1)] public byte ChannelCount { get; set; }
            [FieldOrder(2)] [FieldCount(nameof(ChannelCount))] public List<IPS_RESULT_SERVICESTATE_CHANNEL> Channels { get; set; }
        }

        public class IPS_RESULT_SERVICESTATE_CHANNEL
        {
            [FieldOrder(0)] public byte GroupIdx { get; set; }
            [FieldOrder(1)] public byte ServerIdx { get; set; }
            [FieldOrder(2)] public ushort OnlinePlayers { get; set; }
            [FieldOrder(3)] public ushort MaxPlayers { get; set; }
            [FieldOrder(4)] [FieldLength(4)] public byte[] PublicIP { get; set; }
            [FieldOrder(5)] public ushort PublicPort { get; set; }
            [FieldOrder(6)] public long ServiceType { get; set; }
        }
    }
}
