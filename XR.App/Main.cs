using System;
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
        
        private const string _regexImportFrom = @"ImportFrom\(""(([a-zA-Z]:\\[\\\S|*\S]?.*)|([\w+]+\:\/\/)?([\w\d-]+\.)*[\w-]+[\.\:]\w+([\/\?\=\&\#]?[\w-]+)*\/?)""\);";

        internal static void RunFile(string location)
        {
            string rawData = GetFileRaw(location);

            string source = ParseFile(rawData);

            PrintLnC($"{location} Compiling...",ConsoleColor.White);
            _ = new Compiler().Build("program", source).Run();

            //var file = Path.Combine(AppContext.BaseDirectory, "program.dll");
            //if (File.Exists(file))
            //    File.Delete(file);
            //PrintLn($"{file} saved!");

            //File.WriteAllBytes(file, compiler.SourceDetail.BuildCode);

        }

        private static string ParseFile(string file)
        {
            file = ProcessImportFrom(file);

            var cacheFile = file;

            var extrateds = Regex.Split(file, _regexGetKeys);
            var extrated = extrateds.Where(x => !x.IsNull()).ToArray().FirstOrDefault();
            // remove extrated part
            cacheFile = cacheFile.Replace(extrated, string.Empty);
            extrated = extrated.TrimStart();

            var fmtSource = Templates.MainProgramStr.Replace("{code}", extrated.Replace("\r\n", "\r\n         "));

            var mountSource = $"{cacheFile}\n{fmtSource}";

            var finalSource = $"{Templates.DefaultUsings}\n\n{mountSource}";

            return finalSource;
        }
        
        private static string ProcessImportFrom(string file)
        {   
            var matchs = Regex.Matches(file, _regexImportFrom);
            if (matchs == null || matchs.Count == 0)
                return file;

            foreach (var match in matchs)
            {
                var url = match.ToString().Replace($"{Common.ImportFromDefName}(", string.Empty)
                    .Replace("\"",string.Empty)
                    .Replace("(",string.Empty)
                    .Replace(");", string.Empty);

                var code = GetFileRaw(url);

                file = file.Replace(Regex.Match(file,_regexImportFrom).Value, code);

            }

            return file;
        }

        private static string GetFileRaw(string location)
        {
            string sourceContent;
            location = location.TrimEx();
            if (location.ContainsAny("http://", "https://"))
            {
                PrintLnC($"{location} Downloading...",ConsoleColor.White);
                sourceContent = Utilities.GetTextFileAsync(location).Result;
            }
            else
            {
                if (!location.EndsWith(".xr"))
                    throw new OptionException("extension (.xr) mandatory", string.Empty);

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
