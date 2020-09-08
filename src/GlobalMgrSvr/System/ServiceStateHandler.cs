using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using HPT.Common;

namespace GlobalMgrSvr.System
{
    class ServiceStateHandler
    {
        public static ServiceStateHandler Instance => Singleton<ServiceStateHandler>.I;

        private readonly ConcurrentDictionary<byte, ConcurrentDictionary<byte, ServiceContext>> _servicesContext;

        public ServiceStateHandler()
        {
            _servicesContext = new ConcurrentDictionary<byte, ConcurrentDictionary<byte, ServiceContext>>();
        }

        public KeyValuePair<byte, KeyValuePair<byte, ServiceContext>[]>[] GetAll()
        {
            return _servicesContext
                .ToDictionary(k => k.Key, v => v.Value.ToArray())
                .ToArray();
        }

        public bool TryAdd(ServiceContext serviceContext)
        {
            if (!_servicesContext.ContainsKey(serviceContext.GroupIdx))
            {
                if (!_servicesContext.TryAdd(serviceContext.GroupIdx, new ConcurrentDictionary<byte, ServiceContext>()))
                    return false;
            }

            if (!_servicesContext[serviceContext.GroupIdx].TryAdd(serviceContext.ServerIdx, serviceContext))
               return false;

            return true;
        }

        public bool TryGet(byte groupIdx, byte serverIdx, out ServiceContext serviceContext)
        {
            serviceContext = default;

            return _servicesContext.TryGetValue(groupIdx, out var services) && services.TryGetValue(serverIdx, out serviceContext);
        }

        public bool TryRemove(byte groupIdx, byte serverIdx, out ServiceContext serviceContext)
        {
            serviceContext = default;

            return _servicesContext.TryGetValue(groupIdx, out var services) && services.TryRemove(serverIdx, out serviceContext);
        }
    }
}
