using HPT.Common;

namespace Share.Encryption
{
    public class CabalEncryption
    {
        public static CabalEncryption Instance => Singleton<CabalEncryption>.I;

        public const uint KEY = 0x4C4DE297;

        private readonly uint[] _masks = { 0x00000000, 0x000000FF, 0x0000FFFF, 0x00FFFFFF };
        private readonly byte[] _keychain = new byte[0x20000];

        public CabalEncryption()
        {
            GenerateKeychain(0x8F54C37B, 0, 0x4000);
            GenerateKeychain(KEY, 0x4000, 0x4000);
        }

        public unsafe void ChangeStep(ICabalEncryptable encryptable, in uint step)
        {
            encryptable.Mul = 2;
            encryptable.Step = step - 1;

            if (encryptable.Step < 0)
                encryptable.Step += 0x4000;

            fixed (byte* pkeychain = _keychain)
                encryptable.HeaderXor = *((uint*)&pkeychain[encryptable.Step * encryptable.Mul * 4]);
        }

        public unsafe void Encrypt(ref byte[] packet, in int size)
        {
            fixed (byte* ppacket = packet, pkeychain = _keychain)
            {
                *((uint*)ppacket) ^= 0x7AB38CF1;
                var token = *((uint*)ppacket);
                token &= 0x3FFF;
                token *= 4;
                token = *((uint*)&pkeychain[token]);

                int i, t = (size - 4) / 4;    // Process in blocks of 32 bits (4 bytes)

                for (i = 4; t > 0; i += 4, t--)
                {
                    var t1 = *((uint*)&ppacket[i]);
                    t1 ^= token;
                    *((uint*)&ppacket[i]) = t1;

                    t1 &= 0x3FFF;
                    t1 *= 4;
                    token = *((uint*)&pkeychain[t1]);
                }

                token &= _masks[((size - 4) & 3)];
                *((uint*)&ppacket[i]) ^= token;
            }
        }

        public unsafe void Decrypt(ICabalEncryptable encryptable, ref byte[] buffer, in int size, in int index = 0)
        {
            var header = (uint)size;
            header <<= 16;
            header += 0xB7E2;

            fixed (byte* ppacket = &buffer[index], pkeychain = _keychain)
            {
                var token = *((uint*)ppacket);
                token &= 0x3FFF;
                token *= encryptable.Mul;
                token *= 4;
                token = *((uint*)&pkeychain[token]);
                *((uint*)ppacket) = header;

                int i, t = (size - 8) / 4;    // Process in blocks of 32 bits (4 bytes)

                for (i = 8; t > 0; i += 4, t--)
                {
                    var t1 = *((uint*)&ppacket[i]);
                    token ^= t1;
                    *((uint*)&ppacket[i]) = token;

                    t1 &= 0x3FFF;
                    t1 *= encryptable.Mul;
                    t1 *= 4;
                    token = *((uint*)&pkeychain[t1]);
                }

                token &= _masks[((size - 8) & 3)];
                *((uint*)&ppacket[i]) ^= token;

                *((uint*)&ppacket[4]) = 0;

                encryptable.Step += 1;
                encryptable.Step &= 0x3FFF;
                encryptable.HeaderXor = *((uint*)&pkeychain[encryptable.Step * encryptable.Mul * 4]);
            }
        }

        public static int GetPacketSize(ICabalEncryptable encryptable, uint header)
        {
            if (encryptable.Mul == 1)
                return 0x0E;

            header ^= encryptable.HeaderXor;
            header >>= 16;

            return (int)header;
        }

        private unsafe void GenerateKeychain(uint key, int position, int size)
        {
            uint ret2;
            uint ret3;
            uint ret4;

            for (int i = position; i < position + size; i++)
            {
                ret4 = key * 0x2F6B6F5;
                ret4 += 0x14698B7;
                ret2 = ret4;
                ret4 >>= 0x10;
                ret4 *= 0x27F41C3;
                ret4 += 0x0B327BD;
                ret4 >>= 0x10;

                ret3 = ret2 * 0x2F6B6F5;
                ret3 += 0x14698B7;
                key = ret3;
                ret3 >>= 0x10;
                ret3 *= 0x27F41C3;
                ret3 += 0x0B327BD;
                ret3 &= 0xFFFF0000;

                ret4 |= ret3;

                fixed (byte* pkeychain = _keychain)
                    *((uint*)&pkeychain[i * 4]) = ret4;
            }
        }
    }
}
