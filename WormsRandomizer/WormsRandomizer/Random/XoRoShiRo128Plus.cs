using System;
using System.Security.Cryptography;
using System.Text;

namespace WormsRandomizer.Random
{
    //Based on http://xoshiro.di.unimi.it/xoroshiro128plus.c (Public domain - http://creativecommons.org/publicdomain/zero/1.0/)
    internal class XoRoShiRo128Plus : IRng
    {
        private ulong _s0;
        private ulong _s1;

        public XoRoShiRo128Plus(string seed)
        {
            using (var md5 = MD5.Create())
            {
                var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(seed));
                _s0 = BitConverter.ToUInt64(bytes, 0);
                _s1 = BitConverter.ToUInt64(bytes, 8);
            }
        }

        public int Next(int maxValue)
        {
            var x = NextInt();

            var overlap = int.MaxValue % maxValue;
            while (x >= int.MaxValue - overlap)
            {
                x = NextInt();
            }

            return x % maxValue;
        }

        public int Next(int lower, int upper)
        {
            return Next(upper - lower) + lower;
        }

        private int NextInt() => (int)(NextULong() >> 33);

        private ulong NextULong()
        {
            var s0 = _s0;
            var s1 = _s1;
            var result = unchecked(s0 + s1);

            s1 ^= s0;
            _s0 = RotateLeft(s0, 24) ^ s1 ^ (s1 << 16);
            _s1 = RotateLeft(s1, 37);
            return result;
        }

        private static ulong RotateLeft(ulong x, int k)
        {
            return (x << k) | (x >> (64 - k));
        }
    }
}