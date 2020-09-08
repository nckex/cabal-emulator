using HPT.Logging;
using HPT.Network;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DBAgent
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
                Procdef.RegisterProcs();

                var dbAgentTcpServer = DBAgentServer.Instance;
                dbAgentTcpServer.Listen(
                    new TcpServerOptions(
                        Config.Instance.Listen.IP,
                        Config.Instance.Listen.Port,
                        Config.Instance.Listen.NoDelay,
                        Config.Instance.Listen.Backlog,
                        Config.Instance.Listen.HeaderSize),
                    (socket, sessionId) => new DBAgentSession(socket, sessionId));

                var cancellation = new CancellationTokenSource();

                var t1 = dbAgentTcpServer.StartAcceptAsync(cancellation.Token);

                await Task.WhenAll(t1);

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
