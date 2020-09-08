using BinarySerialization;
using System.Threading.Tasks;
using HPT.Common;
using System;
using System.Buffers;
using System.IO;
using static Share.Protosdef;

namespace Share.Serde
{
    public class IPCSerializer
    {
        public static IPCSerializer Instance => Singleton<IPCSerializer>.I;

        private readonly BinarySerializer _binarySerializer;

        public IPCSerializer()
        {
            _binarySerializer = new BinarySerializer();
        }

        public async Task<Serialized> SerializeAsync<T>(T context, ushort opcode)
            where T : IPS_HEADER
        {
            var contextSize = (ushort)await _binarySerializer.SizeOfAsync(context);

            context.TotalSize = contextSize;
            context.Opcode = opcode;

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
