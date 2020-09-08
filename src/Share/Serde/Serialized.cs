using System;
using System.Buffers;

namespace Share.Serde
{
    public class Serialized : IDisposable
    {
        public ArraySegment<byte> Buffer { get; }

        private readonly byte[] _pooledBuffer;

        public Serialized(byte[] pooledBuffer, int originalSize)
        {
            _pooledBuffer = pooledBuffer;

            Buffer = new ArraySegment<byte>(pooledBuffer, 0, originalSize);
        }

        public static implicit operator ArraySegment<byte>(Serialized serialized)
        {
            return serialized.Buffer;
        }

        #region IDisposable Support
        private bool disposedValue = false; // Para detectar chamadas redundantes

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    ArrayPool<byte>.Shared.Return(_pooledBuffer);
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
