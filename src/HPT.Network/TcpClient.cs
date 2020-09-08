using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;

namespace HPT.Network
{
    public abstract class TcpClient : TcpConnection
    {
        protected readonly ILogger<TcpClient> _logger;

        private bool _isRunning;

        private TcpClientOptions _tcpClientOptions;

        protected TcpClient() : base(new Socket(SocketType.Stream, ProtocolType.Tcp))
        {
            _logger = CreateLogger();
        }

        protected abstract ILogger<TcpClient> CreateLogger();

        public void Connect(TcpClientOptions tcpClientOptions)
        {
            _tcpClientOptions = tcpClientOptions;

            var policy = Policy
                          .Handle<SocketException>()
                          .WaitAndRetry(tcpClientOptions.Retry,
                            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                            (exception, timeSpan, retryCount, context) => 
                            {
                                _logger.LogError(exception, $"Failed to connect {tcpClientOptions.Address}:{tcpClientOptions.Port} ({retryCount})");
                            });

            _socket.NoDelay = tcpClientOptions.NoDelay;
            var execution = policy.ExecuteAndCapture(() => _socket.Connect(new IPEndPoint(IPAddress.Parse(tcpClientOptions.Address), tcpClientOptions.Port)));
            if (execution.FinalException != null)
                throw execution.FinalException;

            _logger.LogInformation($"The client has been successfully connected to {tcpClientOptions.Address}:{tcpClientOptions.Port}");
        }

        public async Task StartProcessAsync()
        {
            if (_isRunning)
                return;

            _isRunning = true;

            while (true)
            {
                try
                {
                    await ProcessPipeAsync(_tcpClientOptions.HeaderSize);
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, $"The client pipeline was aborted");
                }
                finally
                {
                    _logger.LogInformation($"The client has been disconnected");
                }

                if (_tcpClientOptions.AbortOnDisconnect)
                {
                    break;
                }

                Connect(_tcpClientOptions);
            }

            _socket.AbortSocket();

            _isRunning = false;
        }
    }
}
