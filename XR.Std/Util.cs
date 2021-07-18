using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using XR.Std.Extensions;

namespace XR.Std
{
    public static class Util
    {
        public static string AssemblyDirectory
        {
            get
            {
                string location = null;
#if NET46 //|| NETStandard21
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                location = Path.GetDirectoryName(path);
#else
                location = Assembly.GetExecutingAssembly().Location;
                if (location.IsNull())
                    location = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
#endif
                return location;
            }
        }

        public static byte[] GetEmbeddedResourceBytes(string resourceName)
        {
            var manifestResources = Assembly.GetCallingAssembly().GetManifestResourceNames();
            var resourceFullName = manifestResources.FirstOrDefault(N => N.Contains(resourceName));

            if (resourceFullName != null)
            {
                return Path.GetExtension(manifestResources.FirstOrDefault(N => N.Contains(resourceName))) == ".comp"
                    ? Decompress(Assembly.GetCallingAssembly().GetManifestResourceStream(resourceFullName).ReadFully())
                    : Assembly.GetCallingAssembly().GetManifestResourceStream(resourceFullName).ReadFully();
            }

            return null;
        }

        public static byte[] ReadFully(this Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public static byte[] Compress(byte[] Bytes)
        {
            byte[] compressedBytes;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress))
                {
                    deflateStream.Write(Bytes, 0, Bytes.Length);
                }
                compressedBytes = memoryStream.ToArray();
            }
            return compressedBytes;
        }

        public static byte[] Decompress(byte[] compressed)
        {
            using MemoryStream inputStream = new MemoryStream(compressed.Length);
            inputStream.Write(compressed, 0, compressed.Length);
            inputStream.Seek(0, SeekOrigin.Begin);
            using (MemoryStream outputStream = new MemoryStream())
            {
                using (DeflateStream deflateStream = new DeflateStream(inputStream, CompressionMode.Decompress))
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    while ((bytesRead = deflateStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        outputStream.Write(buffer, 0, bytesRead);
                    }
                }
                return outputStream.ToArray();
            }
        }

        public static string GetEnvLoggerFile(string varName)
        {
            try
            {
                if (varName.IsNull())
                    throw new ArgumentNullException(varName);

                return Environment.GetEnvironmentVariable(varName, EnvironmentVariableTarget.Machine);
            }
            catch { }

            return null;
        }

        public static (bool IsActive, string Name) GetDevEnv()
        {
            try
            {
                var env = Environment.GetEnvironmentVariable("_env", EnvironmentVariableTarget.Machine);
                if (!env.IsNull())
                {
                    switch (env.ToLower())
                    {
                        case "dev":
                            return (true, "Development");
                        case "prod":
                            return (false, "Production");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            return (false, "Production");
        }

        public static string GetFullError(Exception ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine(ex.Message);

            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
                sb.AppendLine(ex.Message);
            }
            return sb.ToString();
        }

        public static string GetFullStackTraceError(Exception ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine(ex.StackTrace);

            if (ex.InnerException != null)
            {
                var fullStack = ex.InnerException.StackTrace;
                sb.AppendLine(fullStack);
            }
            return sb.ToString();
        }
    }
}
