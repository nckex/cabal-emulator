using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using HPT.Logging;
using HPT.Network;

namespace LoginSvr
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
                // Register procedures
                Procdef.RegisterProcs();

                var loginSvrTcpServer = LoginSvrServer.Instance;
                loginSvrTcpServer.Listen(
                    new TcpServerOptions(
                        Config.Instance.Listen.IP,
                        Config.Instance.Listen.Port,
                        Config.Instance.Listen.NoDelay,
                        Config.Instance.Listen.Backlog,
                        Config.Instance.Listen.HeaderSize),
                    (socket, sessionId) => new LoginSvrSession(socket, sessionId));

                var gmsTcpClient = GlobalMgrClient.Instance;
                gmsTcpClient.Connect(
                    new TcpClientOptions(
                        Config.Instance.GlobalMgr.IP,
                        Config.Instance.GlobalMgr.Port,
                        Config.Instance.GlobalMgr.NoDelay,
                        Config.Instance.GlobalMgr.Retry,
                        Config.Instance.GlobalMgr.AbortOnDisconnect));

                var cancellation = new CancellationTokenSource();

                var loginAccept = loginSvrTcpServer.StartAcceptAsync(cancellation.Token);
                var gmsProcess = gmsTcpClient.StartProcessAsync();

                await Task.WhenAll(loginAccept, gmsProcess);

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
