using Microsoft.Extensions.Logging;
using HPT.Common;
using HPT.Logging;
using HPT.Network;

namespace GlobalMgrSvr
{
    class GlobalMgrServer : TcpServer<GlobalMgrSession>
    {
        public static GlobalMgrServer Instance => Singleton<GlobalMgrServer>.I;

        protected override ILogger<TcpServer<GlobalMgrSession>> CreateLogger()
        {
            return Singleton<CustomLogger<GlobalMgrServer>>.I;
        }
    }
}
