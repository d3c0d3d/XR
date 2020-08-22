using System;
using System.Collections.Generic;
using System.Text;
using XR.Kernel;
using XR.Kernel.OptionCommand;
using XR.Kernel.Extensions;
using static XR.Kernel.Util.ConsoleHelpers;
using System.IO;
using XR.Kernel.Util;
using System.Text.RegularExpressions;
using System.Linq;

namespace XR.App
{
    internal class Main
    {
        private const string _regexGetKeys = @"[^}]*}";

        internal static void RunFile(string location)
        {
            string rawData = GetFileRaw(location);

            string source = ParseFile(rawData);

            PrintLn($"{location} Compiling...");

            _ = new Compiler()                
                .AddSource("program", source)
                .Build()
                .Run();
        }

        private static string ParseFile(string file)
        {
            var cacheFile = file;

            var extrated = Regex.Split(file, _regexGetKeys);
            extrated = extrated.Where(x => !x.IsNull()).ToArray();

            cacheFile = cacheFile.Replace(extrated.FirstOrDefault(), string.Empty);

            var fmtSource = Templates.MainProgramStr.Replace("{code}", extrated.FirstOrDefault());

            var finalSource = $"{cacheFile}\n{fmtSource}";

            return finalSource;
        }

        private static string GetFileRaw(string location)
        {
            string sourceContent;
            if (location.ContainsAny("http://", "https://"))
            {
                PrintLn($"{location} Downloading...");
                sourceContent = Utilities.GetTextFileAsync(location).Result;
            }
            else
            {
                sourceContent = File.ReadAllText(location);
            }

            return sourceContent;
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
