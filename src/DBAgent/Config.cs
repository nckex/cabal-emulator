using HPT.Common;
using Share.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace DBAgent
{
    class Config
    {
        public static Config Instance => Singleton<Config>.I;

        public ListenSectionConfig Listen { get; set; }
        public DBAgentSection DBAgent { get; set; }
        public LogSectionConfig Log { get; set; }

        public class DBAgentSection
        {
            public string ConnectionString { get; set; }
        }
    }
}
