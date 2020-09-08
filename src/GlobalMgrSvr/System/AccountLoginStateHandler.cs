using System.Collections.Generic;
using System.Collections.Concurrent;
using HPT.Common;

namespace GlobalMgrSvr.System
{
    class AccountLoginStateHandler
    {
        public static AccountLoginStateHandler Instance => Singleton<AccountLoginStateHandler>.I;

        private readonly ConcurrentDictionary<int, ConcurrentDictionary<(byte, byte), (ushort, uint)>> _accounts;

        public AccountLoginStateHandler()
        {
            _accounts = new ConcurrentDictionary<int, ConcurrentDictionary<(byte, byte), (ushort, uint)>>();
        }

        public bool IsStateActiveOnService(int userNum, byte groupIdx, byte serverIdx)
        {
            return _accounts.TryGetValue(userNum, out var currentState) && currentState.TryGetValue((groupIdx, serverIdx), out _);
        }

        public bool TryGetCurrentState(int userNum, out KeyValuePair<(byte groupIdx, byte serverIdx), (ushort sessionId, uint sessionTime)>[] currentState)
        {
            if (!_accounts.TryGetValue(userNum, out var inMemoryCurrentState))
            {
                currentState = default;
                return false;
            }

            currentState = inMemoryCurrentState.ToArray();
            return true;
        }

        public bool TryAddState(int userNum, byte groupIdx, byte serverIdx, ushort sessionId, uint sessionTime)
        {
            if (!ServiceStateHandler.Instance.TryGet(groupIdx, serverIdx, out var serviceContext))
                return false;

            if (!_accounts.ContainsKey(userNum))
            {
                if (!_accounts.TryAdd(userNum, new ConcurrentDictionary<(byte, byte), (ushort, uint)>()))
                    return false;
            }

            if (!_accounts[userNum].TryAdd((groupIdx, serverIdx), (sessionId, sessionTime)))
                return false;

            serviceContext.IncrementOnlineCount();
            return true;
        }

        public bool TryRemoveState(int userNum)
        {
            if (!_accounts.TryRemove(userNum, out var currentState))
                return false;

            foreach ((byte groupIdx, byte serverIdx) in currentState.Keys)
            {
                if (ServiceStateHandler.Instance.TryGet(groupIdx, serverIdx, out var serviceContext))
                    serviceContext.DecrementOnlineCount();
            }

            return true;
        }

        public bool TryRemoveState(int userNum, byte groupIdx, byte serverIdx)
        {
            if (!_accounts.TryGetValue(userNum, out var currentState))
                return false;

            if (!currentState.TryRemove((groupIdx, serverIdx), out _))
                return false;

            if (ServiceStateHandler.Instance.TryGet(groupIdx, serverIdx, out var serviceContext))
                serviceContext.DecrementOnlineCount();

            return true;
        }
    }
}
