using System;
using System.Collections.Generic;
using HPT.Common;

namespace HPT.Handlers
{
    public class MethodHandler<TKey, TDelegate>
        where TKey : notnull
        where TDelegate : Delegate
    {
        public static MethodHandler<TKey, TDelegate> Instance => Singleton<MethodHandler<TKey, TDelegate>>.I;

        private readonly Dictionary<TKey, TDelegate> _methods;

        public MethodHandler()
        {
            _methods = new Dictionary<TKey, TDelegate>();
        }

        public MethodHandler<TKey, TDelegate> Register(TKey key, TDelegate delegateAction)
        {
            _methods.Add(key, delegateAction);
            return this;
        }

        public bool TryGet(TKey key, out TDelegate delegateAction)
        {
            return _methods.TryGetValue(key, out delegateAction);
        }
    }
}
