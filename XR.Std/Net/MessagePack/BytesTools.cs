using System;
using System.Text;

namespace XR.Std.Net.MessagePack
{
    public class BytesTools
    {
        static readonly UTF8Encoding utf8Encode = new UTF8Encoding();

        public static byte[] GetUtf8Bytes(string str)
        {

            return utf8Encode.GetBytes(str);
        }

        public static string GetString(byte[] utf8Bytes)
        {
            return utf8Encode.GetString(utf8Bytes);
        }

        public static string BytesAsString(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                sb.Append(string.Format("{0:D3} ", b));
            }
            return sb.ToString();
        }

        public static string BytesAsHexString(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                sb.Append(string.Format("{0:X2} ", b));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Exchange byte array data Can be used for high and low data exchange
        /// </summary>
        /// <param name="value">The byte array to swap</param>
        /// <returns>Return the exchanged data</returns>
        public static byte[] SwapBytes(byte[] value)
        {
            byte[] r = new byte[value.Length];
            int j = value.Length - 1;
            for (int i = 0; i < r.Length; i++)
            {
                r[i] = value[j];
                j--;
            }
            return r;
        }

        public static byte[] SwapInt64(long value)
        {   
            return SwapBytes(BitConverter.GetBytes(value));
        }

        public static byte[] SwapInt32(int value)
        {
            byte[] r = new byte[4];
            r[3] = (byte)value;
            r[2] = (byte)(value >> 8);
            r[1] = (byte)(value >> 16);
            r[0] = (byte)(value >> 24);
            return r;
        }

        public static byte[] SwapInt16(short value)
        {
            byte[] r = new byte[2];
            r[1] = (byte)value;
            r[0] = (byte)(value >> 8);
            return r;
        }

        public static byte[] SwapDouble(double value)
        {
            return SwapBytes(BitConverter.GetBytes(value));
        }
    }
}