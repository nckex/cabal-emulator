using System.Threading.Tasks;
using BinarySerialization;
using HPT.Common;
using System;
using System.Buffers;
using System.IO;

namespace Share.Serde
{
    public class CustomSerializer
    {
        public static CustomSerializer Instance => Singleton<CustomSerializer>.I;

        private readonly BinarySerializer _binarySerializer;

        public CustomSerializer()
        {
            _binarySerializer = new BinarySerializer();
        }

        public async Task<Serialized> SerializeAsync<T>(T context)
            where T : class
        {
            var contextSize = (ushort)await _binarySerializer.SizeOfAsync(context);

            var pooledBuffer = ArrayPool<byte>.Shared.Rent(contextSize);
            try
            {
                using var stream = new MemoryStream(pooledBuffer);
                await _binarySerializer.SerializeAsync(stream, context);

                return new Serialized(pooledBuffer, contextSize);
            }
            catch
            {
                ArrayPool<byte>.Shared.Return(pooledBuffer);
                throw;
            }
        }

        public async Task<T> DeserializeAsync<T>(byte[] value)
        {
            return await _binarySerializer.DeserializeAsync<T>(value);
        }

        public async Task<T> DeserializeAsync<T>(ArraySegment<byte> value)
        {
            return await _binarySerializer.DeserializeAsync<T>(value.Array);
        }
    }
}
