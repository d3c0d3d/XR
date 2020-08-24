namespace XR.Kernel
{
    public static class Templates
    {
        public static string[] DefaultUsings = new string[] { 
            "using System;",
            "using XR.Kernel.Util;",
            "using static XR.Kernel.Util.ConsoleHelpers;"
        };

        public const string MainProgramStr =
@"public class Program
{
    public static void Main()
    {
         {code}
    }

    {methods}
}";
    }
}
