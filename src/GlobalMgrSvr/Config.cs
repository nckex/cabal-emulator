using HPT.Common;
using Share.Config;

namespace GlobalMgrSvr
{
    class Config
    {
        public static Config Instance => Singleton<Config>.I;

        public ListenSectionConfig Listen { get; set; }
        public GlobalMgrSection GlobalMgr { get; set; }
        public LogSectionConfig Log { get; set; }

        public class GlobalMgrSection
        {
            public string ConnectionString { get; set; }
            public bool SubpwdFailBlock { get; set; }
            public byte SubpwdFailLimit { get; set; }
        }
    }
}
