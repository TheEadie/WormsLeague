using System;
using System.Security.Cryptography;
using System.Text;

namespace WormsRandomizer.Random
{
    internal class SystemRandom : IRng
    {
        private readonly System.Random _rng;

        public SystemRandom(string seed)
        {
            using (var md5 = MD5.Create())
            {
                var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(seed));
                _rng = new System.Random(BitConverter.ToInt32(bytes, 0));
            }
        }

        public int Next(int max) => _rng.Next(max);

        public int Next(int lower, int upper) => _rng.Next(lower, upper);
    }
}
