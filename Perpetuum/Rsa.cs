//#define PUBLICWORK

using System.Linq;
using System.Numerics;

namespace Perpetuum
{
    public static class Rsa
    {
        private static readonly byte[] _modulusKey = new byte[]
#if !PUBLICWORK
            {
                0xd5,0x36,0xee,0x26,0x9b,0x07,0x91,0x03,
                0xfc,0xd3,0x37,0x36,0x6c,0x2b,0xe6,0x98,
                0x19,0xe0,0xcf,0x44,0xee,0x3c,0x51,0xe2,
                0x7c,0x00,0x05,0x3f,0x9c,0xd6,0x0a,0xc1,
                0x26,0xea,0xbd,0x40,0x96,0x9a,0xb4,0xe7,
                0xb4,0xdf,0xc4,0x20,0x2d,0x0a,0x6e,0x30,
                0x7d,0xcb,0x91,0x03,0x6b,0x4e,0x06,0x9f,
                0x5d,0x53,0x42,0x58,0xe0,0x98,0xc2,0xf3,
                0xdb,0x6b,0x5f,0xf3,0x89,0x90,0xb6,0x00,
                0x56,0xa8,0xab,0x85,0x7f,0xdd,0x2e,0x2d,
                0xe9,0x56,0x6d,0xe2,0xa6,0x70,0x0a,0xee,
                0xff,0xb9,0xe7,0x09,0x25,0x0d,0x54,0xf7,
                0xd6,0x31,0x77,0x19,0x51,0x77,0xf1,0x5b,
                0x0e,0x14,0x53,0x92,0xab,0x7a,0xd9,0x79,
                0xfc,0xd9,0xc2,0x64,0x61,0xe3,0x4b,0xc9,
                0x68,0xa7,0x27,0xe6,0xd6,0xd2,0x59,0xb2,
                0x00,
            };
#else
            {
                0xd5,0x36,0xee,0x26,0x9b,0x07,0x91,0x03,
                0xfc,0xd3,0x37,0x36,0x6c,0x2b,0xe6,0xc4,
                0x20,0x2d,0x0a,0x6e,0x30,0x7d,0xcb,0x91,
                0x53,0xb2,0x00,
            };
#endif

        private static readonly byte[] _privateExponentKey = new byte[]
#if !PUBLICWORK
            {
                0x19,0xe6,0x48,0x86,0x7a,0x07,0xe5,0xe2,
                0x3a,0x5f,0x16,0x0e,0x8d,0x2a,0x01,0x7b,
                0xfd,0x76,0x1e,0xf7,0x8a,0xb5,0xeb,0x08,
                0x24,0x97,0x24,0xd9,0xd6,0x8b,0xde,0x87,
                0x74,0x0f,0x1c,0xac,0xea,0x13,0x7a,0x6d,
                0x13,0x9f,0x5c,0xf4,0xe6,0x05,0x26,0xd2,
                0xeb,0xa0,0x90,0x1b,0xa2,0x7d,0xe1,0xc6,
                0xb4,0x24,0x41,0xf5,0x5c,0x24,0x7f,0xdd,
                0xd1,0x58,0x12,0x6e,0x44,0xb3,0xf0,0x5a,
                0xce,0xb7,0xe7,0x40,0xa5,0xf2,0x80,0x70,
                0x29,0xde,0x2c,0x6f,0x89,0x20,0x90,0x4b,
                0x5a,0xc6,0xaf,0x8f,0x2d,0x65,0x72,0x8f,
                0x38,0x56,0x8f,0xab,0x24,0x44,0x30,0xc4,
                0xfc,0x88,0x62,0x5a,0x9c,0xfb,0x94,0xa0,
                0x84,0xb3,0xdc,0x34,0x50,0xca,0x89,0x69,
                0x74,0x20,0x99,0xcc,0x83,0x8f,0xe0,0x92,
                0x00,
            };

#else
            {
                0x19,0xe6,0x48,0x86,0x7a,0x07,0xe5,0xe2,
                0x3a,0x5f,0x16,0x0e,0x8d,0x2a,0x01,0x7b,
                0xfd,0x76,0x1e,0xf7,0xe1,0x46,0x34,0x0b,
                0xaf,0x92,0x00,
            };
#endif

        private static readonly BigInteger _modulus = new BigInteger(_modulusKey);
        private static readonly BigInteger _privateExponent = new BigInteger(_privateExponentKey);

        [CanBeNull]
        public static byte[] Decrypt(byte[] input)
        {
            if (input.Length > _modulusKey.Length)
                return null;

            var encryptedData = input.Reverse().ConcatSingle((byte)0).ToArray();

            var output = BigInteger.ModPow(new BigInteger(encryptedData), _privateExponent, _modulus).ToByteArray().Reverse().ToArray();

            return output[0] == 0 ? output.Skip(1).ToArray() : output;
        }

        [CanBeNull]
        public static byte[] Encrypt(byte[] input)
        {
            if (input.Length > _modulusKey.Length)
                return null;

            var encryptedData = input.Reverse().ConcatSingle((byte)0).ToArray();

            var output = BigInteger.ModPow(new BigInteger(encryptedData),0x11, _modulus).ToByteArray().Reverse().ToArray();

            return output[0] == 0 ? output.Skip(1).ToArray() : output;
        }
    }

}