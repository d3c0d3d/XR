using System;
using System.Text;
using XR.Extensions;

namespace XR.Std
{
    public static class Util
    {
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
