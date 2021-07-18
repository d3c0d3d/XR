using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace XR.Std
{
    public static class SHA256
    {
        public static string ComputeHash(string input)
        {
            byte[] data = Encoding.UTF8.GetBytes(input);
            using (SHA256Managed sha = new SHA256Managed())
            {
                data = sha.ComputeHash(data);
            }
            StringBuilder hash = new StringBuilder();
            foreach (byte _byte in data)
                hash.Append(_byte.ToString("x2"));

            return hash.ToString();
        }

        public static byte[] ComputeHash(byte[] input)
        {
            using (SHA256Managed sha = new SHA256Managed())
            {
                return sha.ComputeHash(input);
            }
        }

        /// <summary>
        /// Generate FingerPrint hash of device with 
        /// <see cref="Environment.ProcessorCount"/>, <see cref="Environment.UserName"/>, 
        /// <see cref="Environment.MachineName"/>, <see cref="Environment.OSVersion"/>, <see cref="DriveInfo.TotalSize"/>
        /// </summary>
        /// <param name="includeProcessId">Include <see cref="Process.Id"/> in calculate of hash</param>
        /// <returns></returns>
        public static (string FullHash, string SmallHash) DeviceFingerPrintId(bool includeProcessId = false)
        {
            var pid = includeProcessId ? $"-{Process.GetCurrentProcess().Id}" : string.Empty;

            var result = ComputeHash(string.Concat(pid, Environment.ProcessorCount, Environment.UserName,
                   Environment.MachineName, Environment.OSVersion,
                   new DriveInfo(Path.GetPathRoot(Environment.SystemDirectory)).TotalSize));

            return (result, result.Substring(0, 16));
        }
    }
}
