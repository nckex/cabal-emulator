using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace HPT.Network
{
    public abstract class TcpServer<T>
        where T : TcpSession
    {
        private readonly ILogger _logger;
        private readonly Socket _listenSocket;
        private readonly ConcurrentDictionary<int, T> _sessions;

        private bool _isRunning;
        private int _sessionCounter;

        private TcpServerOptions _tcpServerOptions;
        private Func<Socket, ushort, T> _tcpSessionFactory;

        protected TcpServer()
        {
            _logger = CreateLogger();
            _listenSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            _sessions = new ConcurrentDictionary<int, T>();
        }

        protected abstract ILogger<TcpServer<T>> CreateLogger();

        protected virtual void OnListen()
        {
        }

        protected virtual void OnStartAccept()
        {
        }

        protected virtual void OnStopped()
        {
        }

        public bool TryGetSession(int sessionId, out T tcpSession)
        {
            return _sessions.TryGetValue(sessionId, out tcpSession);
        }

        public bool TryGetSession(int sessionId, uint sessionTime, out T tcpSession)
        {
            return _sessions.TryGetValue(sessionId, out tcpSession) && tcpSession.SessionTime == sessionTime;
        }

        public void Listen(TcpServerOptions tcpSeverOptions, Func<Socket, ushort, T> tcpSessionFactory)
        {
            
            _listenSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, tcpSeverOptions.NoDelay);
            _listenSocket.Bind(new IPEndPoint(IPAddress.Parse(tcpSeverOptions.Address), tcpSeverOptions.Port));
            _listenSocket.Listen(tcpSeverOptions.Backlog);

            _tcpServerOptions = tcpSeverOptions;
            _tcpSessionFactory = tcpSessionFactory;

            _logger.LogInformation($"The listen socket has been successfully binded to {tcpSeverOptions.Address}:{tcpSeverOptions.Port}");

            OnListen();
        }

        public async Task StartAcceptAsync(CancellationToken cancellationToken)
        {
            if (_isRunning)
                return;

            if (_tcpSessionFactory == null)
                throw new ArgumentNullException("_tcpSessionFactory");

            _isRunning = true;
            
            try
            {
                _logger.LogInformation($"The server is now accepting new incoming connections");

                OnStartAccept();

                while (!cancellationToken.IsCancellationRequested)
                {
                    var socket = await _listenSocket.AcceptAsync();
                    _ = ProcessPipeAsync(socket);
                }

                // Cancellation requested, disconnect all sessions and wait for it
                foreach (var session in _sessions.Values)
                    session.Disconnect();

                while (_sessions.Count > 0)
                    await Task.Delay(1000);

                _logger.LogInformation("The server is stopped");

                OnStopped();
            }
            finally
            {
                _isRunning = false;
            }
        }

        private async Task ProcessPipeAsync(Socket socket)
        {
            var currentCount = Interlocked.Increment(ref _sessionCounter);
            if (currentCount > ushort.MaxValue)
            {
                socket.AbortSocket();
                Interlocked.Decrement(ref _sessionCounter);
                return;
            }

            var session = _tcpSessionFactory(socket, (ushort)currentCount);
            if (!_sessions.TryAdd(session.SessionId, session))
            {
                socket.AbortSocket();
                Interlocked.Decrement(ref _sessionCounter);
                return;
            }

            _logger.LogDebug($"Session({session.Signature}) opened");

            try
            {
                await session.ProcessPipeAsync(_tcpServerOptions.HeaderSize);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"The Session({session.Signature}) pipeline was aborted");
            }
            finally
            {
                if (_sessions.TryRemove(session.SessionId, out _))
                {
                    _logger.LogDebug($"Session({session.Signature}) closed");
                }

                socket.AbortSocket();

                Interlocked.Decrement(ref _sessionCounter);
            }
        }
    }
}
