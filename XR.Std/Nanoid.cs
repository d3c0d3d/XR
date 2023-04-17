using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace XR.Std
{
    public static class Nanoid
    {
        private const string DefaultAlphabet = "_-0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static readonly CryptoRandom Random = new();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="alphabet"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static async Task<string> GenerateAsync(string alphabet = DefaultAlphabet, int size = 21) => await Task.Run(() => Generate(Random, alphabet, size));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="alphabet"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string Generate(string alphabet = DefaultAlphabet, int size = 21) => Generate(Random, alphabet, size);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="random"></param>
        /// <param name="alphabet"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static string Generate(Random random, string alphabet = DefaultAlphabet, int size = 21)
        {

            if (random == null)
            {
                throw new ArgumentNullException("random cannot be null.");
            }

            if (alphabet == null)
            {
                throw new ArgumentNullException("alphabet cannot be null.");
            }

            if (alphabet.Length == 0 || alphabet.Length >= 256)
            {
                throw new ArgumentOutOfRangeException("alphabet must contain between 1 and 255 symbols.");
            }

            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException("size must be greater than zero.");
            }

            var mask = (2 << (int)Math.Floor(Math.Log(alphabet.Length - 1) / Math.Log(2))) - 1;
            var step = (int)Math.Ceiling(1.6 * mask * size / alphabet.Length);

            var idBuilder = new char[size];
            int cnt = 0;

            var bytes = new byte[step];
            while (true)
            {

                random.NextBytes(bytes);

                for (var i = 0; i < step; i++)
                {

                    var alphabetIndex = bytes[i] & mask;

                    if (alphabetIndex >= alphabet.Length) continue;
                    idBuilder[cnt] = alphabet[alphabetIndex];
                    if (++cnt == size)
                    {
                        return new string(idBuilder);
                    }

                }

            }

        }
    }

    internal class CryptoRandom : Random
    {
        private static RandomNumberGenerator _r;
        private readonly byte[] _uint32Buffer = new byte[4];
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public CryptoRandom()
        {
            _r = RandomNumberGenerator.Create();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public override void NextBytes(byte[] buffer)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            _r.GetBytes(buffer);
        }
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override double NextDouble()
        {
            _r.GetBytes(_uint32Buffer);
            return BitConverter.ToUInt32(_uint32Buffer, 0) / (1.0 + uint.MaxValue);
        }
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException"></exception>
        public override int Next(int minValue, int maxValue)
        {
            if (minValue > maxValue) throw new ArgumentOutOfRangeException(nameof(minValue));
            if (minValue == maxValue) return minValue;
            var range = (long)maxValue - minValue;
            return (int)((long)Math.Floor(NextDouble() * range) + minValue);
        }
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override int Next()
        {
            return Next(0, int.MaxValue);
        }
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException"></exception>
        public override int Next(int maxValue)
        {
            if (maxValue < 0) throw new ArgumentOutOfRangeException(nameof(maxValue));
            return Next(0, maxValue);
        }
    }
}