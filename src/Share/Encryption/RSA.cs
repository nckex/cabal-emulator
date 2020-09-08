using System;
using System.Buffers;
using System.IO;
using System.Security.Cryptography;
using Microsoft.IO;

namespace Share.Encryption
{
    public class RSA
    {
        public const int KEY_SIZE = 2048;

        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
        private readonly RSACryptoServiceProvider _rsaProvider;

        public ArraySegment<byte> PublicKey => _publicKey;

        private byte[] _publicKey;

        public RSA()
        {
            _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
            _rsaProvider = new RSACryptoServiceProvider(KEY_SIZE, new CspParameters());

            PreparePublicKey();
        }

        public byte[] Decrypt(byte[] encrypted)
        {
            byte[] decrypted = _rsaProvider.Decrypt(encrypted, true);
            return decrypted;
        }

        void PreparePublicKey()
        {
            var parameters = _rsaProvider.ExportParameters(false);

            using var bitStringStream = _recyclableMemoryStreamManager.GetStream();
            var bitStringWriter = new BinaryWriter(bitStringStream);
            bitStringWriter.Write((byte)0x30); // sequence

            var paramsStream = _recyclableMemoryStreamManager.GetStream();
            var paramsWriter = new BinaryWriter(paramsStream);
            EncodeIntegerBigEndian(paramsWriter, parameters.Modulus);   // modulus
            EncodeIntegerBigEndian(paramsWriter, parameters.Exponent);  // exponent
            int paramsLength = (int)paramsStream.Length;
            EncodeLength(bitStringWriter, paramsLength);
            bitStringWriter.Write(paramsStream.GetBuffer(), 0, paramsLength);

            _publicKey = new byte[bitStringStream.Length];
            Array.Copy(bitStringStream.GetBuffer(), _publicKey, bitStringStream.Length);
        }

        void EncodeLength(BinaryWriter stream, int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException("length", "Length must be non-negative");

            if (length < 0x80)
                stream.Write((byte)length); // short form
            else
            {
                // long form
                int temp = length;
                int bytesRequired = 0;

                while (temp > 0)
                {
                    temp >>= 8;
                    bytesRequired++;
                }

                stream.Write((byte)(bytesRequired | 0x80));

                for (int i = bytesRequired - 1; i >= 0; i--)
                {
                    stream.Write((byte)(length >> (8 * i) & 0xFF));
                }
            }
        }

        void EncodeIntegerBigEndian(BinaryWriter stream, byte[] value, bool forceUnsigned = true)
        {
            stream.Write((byte)0x02); // integer
            int prefixZeros = 0;

            for (var i = 0; i < value.Length; i++)
            {
                if (value[i] != 0) break;
                prefixZeros++;
            }

            if (value.Length - prefixZeros == 0)
            {
                EncodeLength(stream, 1);
                stream.Write((byte)0);
            }
            else
            {
                if (forceUnsigned && value[prefixZeros] > 0x7F)
                {
                    // add a prefix zero to force unsigned if the MSB is 1
                    EncodeLength(stream, value.Length - prefixZeros + 1);
                    stream.Write((byte)0);
                }
                else
                    EncodeLength(stream, value.Length - prefixZeros);

                for (var i = prefixZeros; i < value.Length; i++)
                {
                    stream.Write(value[i]);
                }
            }
        }
    }
}
