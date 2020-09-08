using HPT.Common;
using HPT.Logging;
using HPT.Network;
using Microsoft.Extensions.Logging;

namespace LoginSvr
{
    class LoginSvrServer : TcpServer<LoginSvrSession>
    {
        public static LoginSvrServer Instance => Singleton<LoginSvrServer>.I;

        protected override ILogger<TcpServer<LoginSvrSession>> CreateLogger()
        {
            return CustomLogger<LoginSvrServer>.Instance;
        }
    }
}
