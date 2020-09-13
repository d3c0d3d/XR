using System;
using XR.Kernel;
using XR.Std.OptionCommand;
using static XR.Std.Cli;
using XR.Std;
using XR.Std.Logging;
using XR.Kernel.Core;
using System.IO;

namespace XR.App
{
    internal class Main
    {
        public static readonly Logger _logger = LoggerFactory.CreateLogger(LogLevel.Info, Util.GetEnvLoggerFile(Statics.XR_LOGGER_ENV));

        internal static void RunFile(string location, string[] args,string[] assembliesLocation)
        {            
            PrintLnC($"{location} Compiling...",ConsoleColor.White);

            var fileName = Path.GetFileNameWithoutExtension(location);
            
            _ = new CompilerService()
                .Build(fileName, location,Path.GetExtension(".cs") == ".cs",assembliesLocation)
                .Run(args);
        }

        internal static void ShowHelp(OptionSet optionSet)
        {
            PrintLnC("Options:", ConsoleColor.Gray);
            Console.ForegroundColor = ConsoleColor.Gray;
            optionSet.WriteOptionDescriptions(Console.Out, "cmd=");
            Console.ResetColor();
        }

        internal static void Exit()
        {
            PrintLnC("Exiting...", ConsoleColor.Yellow);
            Environment.Exit(0);
        }
    }
}
