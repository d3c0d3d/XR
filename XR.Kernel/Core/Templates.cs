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
            "using System.Linq.Expressions;",
            "using System.Collections.Generic;",
            "using System.Reflection;",
            "using System.Threading.Tasks;",
            $"using XR.Kernel.{nameof(Std)};",
            $"using static XR.Kernel.{nameof(Std)}.{nameof(Std.Cli)};",
            $"using static XR.Kernel.{nameof(Std)}.{nameof(Std.Net)};",
        };

        public static string GetMainProgram(bool async)
        {
            return $"public class Program{Environment.NewLine}" +
                $"{{" +
                $"    public static {(async ? "async Task" : "void")} Main(string[] args){Environment.NewLine}" +
                $"    {{" +
                $"         {{code}}{Environment.NewLine}" +
                $"    }}{Environment.NewLine}" +
                $"    {{methods}}{Environment.NewLine}" +
                $"}}";
        }
    }
}
