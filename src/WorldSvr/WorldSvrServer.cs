using HPT.Common;
using HPT.Logging;
using HPT.Network;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace WorldSvr
{
    class WorldSvrServer : TcpServer<WorldSvrSession>
    {
        public static WorldSvrServer Instance => Singleton<WorldSvrServer>.I;

        protected override ILogger<TcpServer<WorldSvrSession>> CreateLogger()
        {
            return CustomLogger<WorldSvrServer>.Instance;
        }
    }
}
