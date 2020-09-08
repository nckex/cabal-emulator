using HPT.Logging;
using HPT.Network;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WorldSvr.System.Game;
using static WorldSvr.Protos.CharLobby;

namespace WorldSvr
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var configurationBuilder = new ConfigurationBuilder();
            var configuration = configurationBuilder
                .AddIniFile(args[0])
                .Build();

            configuration.Bind(Config.Instance);

            if (Enum.TryParse(typeof(LogLevel), Config.Instance.Log.LogLevel.ToString(), out var logLevel))
                CustomLoggerConfigurator.Instance.SetLogLevel((LogLevel)logLevel);

            try
            {
                WorldManager.Instance.InitWorldTeste();

                Procdef.RegisterProcs();

                var worldSvrTcpServer = WorldSvrServer.Instance;
                worldSvrTcpServer.Listen(
                    new TcpServerOptions(
                        Config.Instance.Listen.IP,
                        Config.Instance.Listen.Port,
                        Config.Instance.Listen.NoDelay,
                        Config.Instance.Listen.Backlog,
                        Config.Instance.Listen.HeaderSize),
                    (socket, sessionId) => new WorldSvrSession(socket, sessionId));

                var gmsTcpClient = GlobalMgrClient.Instance;
                gmsTcpClient.Connect(
                    new TcpClientOptions(
                        Config.Instance.GlobalMgr.IP,
                        Config.Instance.GlobalMgr.Port,
                        Config.Instance.GlobalMgr.NoDelay,
                        Config.Instance.GlobalMgr.Retry,
                        Config.Instance.GlobalMgr.AbortOnDisconnect));

                var dbaTcpClient = DBAgentClient.Instance;
                dbaTcpClient.Connect(
                    new TcpClientOptions(
                        Config.Instance.DBAgent.IP,
                        Config.Instance.DBAgent.Port,
                        Config.Instance.DBAgent.NoDelay,
                        Config.Instance.DBAgent.Retry,
                        Config.Instance.DBAgent.AbortOnDisconnect));

                var cancellation = new CancellationTokenSource();

                var t1 = worldSvrTcpServer.StartAcceptAsync(cancellation.Token);
                var t2 = gmsTcpClient.StartProcessAsync();
                var t3 = dbaTcpClient.StartProcessAsync();

                await Task.WhenAll(t1, t2, t3);

                return 0;
            }
            catch (Exception ex)
            {
                CustomLogger<Program>.Instance.LogCritical(ex, "Aborted");
                return -1;
            }
        }
    }
}
