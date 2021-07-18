using System;
using XR.Std.Extensions;

namespace XR.Kernel.Core
{
    public static class Templates
    {
        public static string[] DefaultUsings = new string[] {
            "using System;",
            "using System.IO;",
            "using System.Text;",
            "using System.Linq;",
            "using System.Reflection;",
            "using System.Diagnostics;",
            "using System.Threading.Tasks;",
            "using System.Linq.Expressions;",
            "using System.Collections.Generic;",
            $"using {nameof(XR)}.{nameof(Std)};",
            $"using static {nameof(XR)}.{nameof(Std)}.{nameof(Std.Cli)};",
            $"using static {nameof(XR)}.{nameof(Std)}.{nameof(Std.OSRuntime)};",
            $"using static {nameof(XR)}.{nameof(Std)}.{nameof(Std.Net)}.{nameof(Std.Net.JsonTools)};"
        };

        public static string MainBody(string code, string methods = null, bool includeProgramClass = true, bool async = false)
        {
            if (includeProgramClass)
                return $"public class Program{Environment.NewLine}" +
                    $"{{" +
                    $"   public static {(async ? "async Task" : "void")} Main(string[] args){Environment.NewLine}" +
                    $"    {{" +
                    $"         {Environment.NewLine}{code}{Environment.NewLine}" +
                    $"    }}" +
                    (!methods.IsNull() ? Environment.NewLine + methods + Environment.NewLine : string.Empty) + $"{Environment.NewLine}}}";
            else return $"{code}{(!methods.IsNull() ? Environment.NewLine + methods : string.Empty)}";
        }
    }
}
