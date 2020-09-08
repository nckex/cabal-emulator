using BinarySerialization;
using System.Collections.Generic;
using static Share.Protosdef;

namespace LoginSvr.Protos
{
    static class ServerState
    {
        public class NFS_SERVERSTATE : S2C_HEADER
        {
            [FieldOrder(0)]
            public byte ServerCount { get; set; }

            [FieldOrder(1)]
            [FieldCount(nameof(ServerCount))]
            public List<NFS_SERVERSTATE_SERVER> Servers { get; set; }
        }

        public class NFS_SERVERSTATE_SERVER
        {
            [FieldOrder(0)]
            public byte GroupIdx { get; set; }

            [FieldOrder(1)]
            public int U0 { get; set; }

            [FieldOrder(2)]
            public short U1 { get; set; }

            [FieldOrder(3)]
            public byte U2 { get; set; }

            [FieldOrder(4)]
            public byte ChannelCount { get; set; }

            [FieldOrder(5)]
            [FieldCount(nameof(ChannelCount))]
            public List<NFS_SERVERSTATE_SERVER_CHANNEL> Channels { get; set; }
        }

        public class NFS_SERVERSTATE_SERVER_CHANNEL
        {
            [FieldOrder(0)]
            public byte GroupIdx { get; set; }

            [FieldOrder(1)]
            public byte ServerIdx { get; set; }

            [FieldOrder(2)]
            public ushort OnlinePlayers { get; set; }

            [FieldOrder(3)]
            [FieldLength(21)]
            public byte U0 { get; set; }

            [FieldOrder(4)]
            public byte U1 { get; set; }

            [FieldOrder(5)]
            public ushort MaxPlayers { get; set; }

            [FieldOrder(6)]
            [FieldLength(65)]
            public string PublicIP { get; set; }

            [FieldOrder(7)]
            public ushort PublicPort { get; set; }

            [FieldOrder(8)]
            public long ServiceType { get; set; }
        }
    }
}
