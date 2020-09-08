using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HPT.Common;
using HPT.Logging;
using Microsoft.Extensions.Logging;
using Share.Serde;
using static Share.Protos.Services;

namespace LoginSvr.System
{
    class ServiceStateHandler
    {
        public static ServiceStateHandler Instance => Singleton<ServiceStateHandler>.I;

        const int POOL_DELAY_MILLISECONDS = 12000;

        private readonly static object _subscribersSync = new object();
        private readonly List<ushort> _subscribers;

        private byte[] _serverlistData;

        private CancellationTokenSource _cancellationTokenSource;
        private Task _poolTask;

        public ServiceStateHandler()
        {
            _subscribers = new List<ushort>();
            _serverlistData = Array.Empty<byte>();
        }

        public void StartPool()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            Task<Task> task = Task.Factory.StartNew(
                function: PoolServerStateAsync,
                cancellationToken: _cancellationTokenSource.Token,
                creationOptions: TaskCreationOptions.LongRunning,
                scheduler: TaskScheduler.Default);

            _poolTask = task.Unwrap();
        }

        public void StopPool()
        {
            _cancellationTokenSource.Cancel();
            _poolTask.Wait();
        }

        public bool Subscribe(ushort sessionId)
        {
            lock(_subscribersSync)
            {
                if (_subscribers.Contains(sessionId))
                    return false;

                _subscribers.Add(sessionId);
                return true;
            }
        }

        public bool Unsubscribe(ushort sessionId)
        {
            lock (_subscribersSync)
            {
                if (!_subscribers.Contains(sessionId))
                    return false;

                _subscribers.Remove(sessionId);
                return true;
            }
        }

        public void StoreServiceState(byte[] serverlistData)
        {
            _serverlistData = serverlistData;
        }

        public async Task BroadcastServerStateAsync()
        {
            foreach (var sessionId in _subscribers)
            {
                if (!LoginSvrServer.Instance.TryGetSession(sessionId, out var session))
                {
                    Unsubscribe(sessionId);
                    continue;
                }

                await SendServerStateToSessionAsync(session);
            }
        }

        public async Task SendServerStateToSessionAsync(LoginSvrSession session)
        {
            try
            {
                await session.SendAsync(_serverlistData);
            }
            catch
            {
                CustomLogger<ServiceStateHandler>.Instance
                    .LogWarning($"Failed to broadcast severstate to session ({session.Signature}), removing from subscribers list");

                Unsubscribe(session.SessionId);

                throw;
            }
        }

        private async Task PoolServerStateAsync()
        {
            byte[] ipsServerState = Array.Empty<byte>();
            using (var ipsServerStateS = await IPCSerializer.Instance.SerializeAsync(new IPS_SERVICESTATE(), Share.Opcode.IPC_SERVICESTATE))
            {
                ipsServerState = ipsServerStateS.Buffer.ToArray();
            }

            do
            {
                await GlobalMgrClient.Instance.SendAsync(ipsServerState);
                await Task.Delay(POOL_DELAY_MILLISECONDS);
            }
            while (!_cancellationTokenSource.IsCancellationRequested);
        }
    }
}
