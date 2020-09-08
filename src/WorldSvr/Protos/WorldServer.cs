using BinarySerialization;
using System;
using System.Collections.Generic;
using System.Text;
using static Share.Protodef;
using static Share.Protosdef;

namespace WorldSvr.Protos
{
    static class WorldServer
    {
        public class S2C_GETSVRTIME : S2C_HEADER
        {
            [FieldOrder(0)]
            public int UnixTimestamp { get; set; }

            [FieldOrder(1)]
            public short Timezone { get; set; }
        }

        public class S2C_SERVERENV : S2C_HEADER
        {
            [FieldOrder(0)]
            public ushort MaxLevel { get; set; }

            [FieldOrder(1)]
            public bool UseDummy { get; set; }

            [FieldOrder(2)]
            public bool AllowCashShop { get; set; }

            [FieldOrder(3)]
            public bool AllowNetCafePoint { get; set; }

            [FieldOrder(4)]
            public ushort NormalChatMinLevel { get; set; }

            [FieldOrder(5)]
            public ushort LoudChatMinLevel { get; set; }

            [FieldOrder(6)]
            public ushort LoudChatMinMasteryLevel { get; set; }

            [FieldOrder(7)]
            public long MaxInventoryAlz { get; set; }

            [FieldOrder(8)]
            public long MaxWarehouseAlz { get; set; }

            [FieldOrder(9)]
            public long U0 { get; set; }

            [FieldOrder(10)]
            public byte U1 { get; set; }

            [FieldOrder(11)]
            public int U2 { get; set; }

            [FieldOrder(12)]
            public short U3 { get; set; }

            [FieldOrder(13)]
            public int U4 { get; set; }

            [FieldOrder(14)]
            public short U5 { get; set; }

            [FieldOrder(15)]
            public short U6 { get; set; }

            [FieldOrder(16)]
            public short U7 { get; set; }

            [FieldOrder(17)]
            public ushort MinRank { get; set; }

            [FieldOrder(18)]
            public short U8 { get; set; }

            [FieldOrder(19)]
            public ushort MaxRank { get; set; }

            [FieldOrder(20)]
            public int U9 { get; set; }

            [FieldOrder(21)]
            public int U10 { get; set; }

            [FieldOrder(22)]
            public int U11 { get; set; }

            [FieldOrder(23)]
            public int U12 { get; set; }

            [FieldOrder(24)]
            public long U13 { get; set; }

            [FieldOrder(25)]
            public long U14 { get; set; }

            [FieldOrder(26)]
            public int U15 { get; set; }

            [FieldOrder(27)]
            public int U16 { get; set; }

            [FieldOrder(28)]
            public int U17 { get; set; }

            [FieldOrder(29)]
            public int U18 { get; set; }

            [FieldOrder(30)]
            public int U19 { get; set; }

            [FieldOrder(31)]
            public int U20 { get; set; }

            [FieldOrder(32)]
            public int U21 { get; set; }

            [FieldOrder(33)]
            public int U22 { get; set; }

            [FieldOrder(34)]
            [FieldLength(255)]
            public byte U23 { get; set; }

            [FieldOrder(35)]
            public int U24 { get; set; }

            [FieldOrder(36)]
            public int U25 { get; set; }

            [FieldOrder(37)]
            public int DeleteCharMaxLevel { get; set; }
        }

        public class NFS_SYSTEMMESSG : S2C_HEADER
        {
            [FieldOrder(0)]
            public SystemMessage SystemMessage { get; set; }

            [FieldOrder(1)]
            public short U0 { get; set; }
        }
    }
}
