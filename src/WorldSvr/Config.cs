using HPT.Common;
using Share.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace WorldSvr
{
    class Config
    {
        public static Config Instance => Singleton<Config>.I;

        public ListenSectionConfig Listen { get; set; }
        public WorldSvrSection WorldSvr { get; set; }
        public ClientSectionConfig GlobalMgr { get; set; }
        public ClientSectionConfig DBAgent { get; set; }
        public LogSectionConfig Log { get; set; }

        public class WorldSvrSection
        {
            public byte GroupIdx { get; set; }
            public byte ServerIdx { get; set; }
            public ushort MaxPlayers { get; set; }
            public long ChannelType { get; set; }
            public ushort MaxLevel { get; set; }
            public bool UseDummy { get; set; }
            public bool AllowCashShop { get; set; }
            public bool AllowNetCafePoint { get; set; }
            public ushort NormalChatMinLevel { get; set; }
            public ushort LoudChatMinLevel { get; set; }
            public ushort LoudChatMinMasteryLevel { get; set; }
            public long MaxInventoryAlz { get; set; }
            public long MaxWarehouseAlz { get; set; }
            public ushort MinRank { get; set; }
            public ushort MaxRank { get; set; }
            public int DeleteCharMaxLevel { get; set; }
        }
    }
}
