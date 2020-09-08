using HPT.Common;
using HPT.Logging;
using HPT.Network;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DBAgent
{
    class DBAgentServer : TcpServer<DBAgentSession>
    {
        public static DBAgentServer Instance => Singleton<DBAgentServer>.I;

        protected override ILogger<TcpServer<DBAgentSession>> CreateLogger()
        {
            return CustomLogger<DBAgentServer>.Instance;
        }
    }
}
