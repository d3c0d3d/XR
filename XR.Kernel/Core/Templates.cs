using System;

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
            $"using {nameof(XR)}.{nameof(Kernel)}.{nameof(Std)};",
            $"using static {nameof(XR)}.{nameof(Kernel)}.{nameof(Std)}.{nameof(Std.Cli)};",
            $"using static {nameof(XR)}.{nameof(Kernel)}.{nameof(Std)}.{nameof(Std.Net)};",
            $"using static {nameof(XR)}.{nameof(Kernel)}.{nameof(Std)}.{nameof(Std.OSRuntime)};",
        };

        public static string MainBody(bool includeProgramClass = true, bool async = false)
        {
            if (includeProgramClass)
                return $"public class Program{Environment.NewLine}" +
                    $"{{" +
                    $"   public static {(async ? "async Task" : "void")} Main(string[] args){Environment.NewLine}" +
                    $"    {{" +
                    $"         {{code}}{Environment.NewLine}" +
                    $"    }}{Environment.NewLine}" +
                    $"    {{methods}}{Environment.NewLine}" +
                    $"}}";
            else return $"{{}}";
        }
    }
}
