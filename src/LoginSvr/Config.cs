using HPT.Common;
using Share.Config;

namespace LoginSvr
{
    class Config
    {
        public static Config Instance => Singleton<Config>.I;

        public ListenSectionConfig Listen { get; set; }
        public LoginSvrSection LoginSvr { get; set; }
        public ClientSectionConfig GlobalMgr { get; set; }
        public LogSectionConfig Log { get; set; }

        public class LoginSvrSection
        {
            public byte ServerIdx { get; set; }
            public bool IgnoreClientVersion { get; set; }
            public int ClientVersion { get; set; }
            public int ClientMagicKey { get; set; }
            public int LoginTimer { get; set; }
            public string URL1 { get; set; }
            public string URL2 { get; set; }
            public string URL3 { get; set; }
            public string URL4 { get; set; }
            public string URL5 { get; set; }
        }
    }
}
