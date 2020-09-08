using System.Threading.Tasks;
using BinarySerialization;
using HPT.Common;
using Share.Encryption;
using System;
using System.Buffers;
using System.IO;
using static Share.Protosdef;

namespace Share.Serde
{
    public class CabalSerializer
    {
        public static CabalSerializer Instance => Singleton<CabalSerializer>.I;

        private readonly BinarySerializer _binarySerializer;

        public CabalSerializer()
        {
            _binarySerializer = new BinarySerializer();
        }

        public async Task<Serialized> SerializeAsync<T>(T context, ushort opcode)
            where T : S2C_HEADER
        {
            var contextSize = (ushort)await _binarySerializer.SizeOfAsync(context);

            context.MagicCode = 0xB7E2;
            context.TotalSize = contextSize;
            context.Opcode = opcode;

            var pooledBuffer = ArrayPool<byte>.Shared.Rent(contextSize);
            try
            {
                using var stream = new MemoryStream(pooledBuffer);
                await _binarySerializer.SerializeAsync(stream, context);

                Singleton<CabalEncryption>.I.Encrypt(ref pooledBuffer, contextSize);

                return new Serialized(pooledBuffer, contextSize);
            }
            catch
            {
                ArrayPool<byte>.Shared.Return(pooledBuffer);
                throw;
            }
        }

        public async Task<Serialized> SerializeUncompressedAsync<T>(T context, ushort opcode)
            where T : S2C_UNCOMPRESSED_HEADER
        {
            var contextSize = (int)await _binarySerializer.SizeOfAsync(context);

            context.MagicCode = 0xC8F3;
            context.TotalSize = contextSize;
            context.Opcode = opcode;

            var pooledBuffer = ArrayPool<byte>.Shared.Rent(contextSize);
            try
            {
                using var stream = new MemoryStream(pooledBuffer);
                await _binarySerializer.SerializeAsync(stream, context);

                Singleton<CabalEncryption>.I.Encrypt(ref pooledBuffer, contextSize);

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
