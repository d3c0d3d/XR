using System;
using XR.Kernel;
using XR.Kernel.OptionCommand;
using XR.Kernel.Extensions;
using static XR.Kernel.Std.Cli;
using System.IO;
using XR.Kernel.Std;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;

namespace XR.App
{
    internal class Main
    {
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
            // get from web or localpath
            var fullcode = ProcessImportFrom(file);

            var cleanupFile = fullcode;

            var codeBuilder = new StringBuilder();

            var usingsList = GetUsingsFromFile(cleanupFile);
            
            foreach (var usinExt in usingsList)
            {
                cleanupFile = cleanupFile.Replace(usinExt, null).TrimEx();

                codeBuilder.AppendLine(usinExt);
            }

            var getClasses = Compiler.GetClassesContent(cleanupFile);
            foreach (var classeContent in getClasses)
            {
                cleanupFile = cleanupFile.Replace(classeContent, null);

                codeBuilder.AppendLine(classeContent);
            }

            var methodsExtracts = Regex.Matches(cleanupFile, Common.RegexMethodCodeBlock);
            string methodsList = null;

            foreach (var method in methodsExtracts)
            {
                // remove method from principal code
                cleanupFile = cleanupFile.Replace(method.ToString(), null);

                methodsList += $"\n{method}";
            }

            cleanupFile = cleanupFile.Replace("\r\n\r\n\r", string.Empty);

            var formatCode = Templates.GetMainProgram(cleanupFile.Contains("await")).Replace("{code}", cleanupFile.Replace("\r\n", "\r\n         "));
            formatCode = formatCode.Replace("{methods}", methodsList);

            codeBuilder.AppendLine(formatCode);

            return codeBuilder.ToString();
        }
        
        private static string ProcessImportFrom(string file)
        {   
            var matchs = Regex.Matches(file, Common.RegexImportFrom);
            if (matchs == null || matchs.Count == 0)
                return file;

            foreach (var match in matchs)
            {
                var url = match.ToString().Replace($"{Common.ImportFromDefName}(", string.Empty)
                    .Replace("\"",string.Empty)
                    .Replace("(",string.Empty)
                    .Replace(");", string.Empty);

                var code = GetFileRaw(url);

                file = file.Replace(Regex.Match(file,Common.RegexImportFrom).Value, code);
            }

            return file;
        }

        private static List<string> GetUsingsFromFile(string file)
        {
            List<string> UsingList = new List<string>();

            var matchs = Regex.Matches(file, Common.RegexUsings);            

            foreach (var match in matchs)
            {
                UsingList.Add(match.ToString());
            }
            // add default usings from template
            foreach (var defUsing in Templates.DefaultUsings)
            {
                if(!UsingList.Contains(defUsing))
                {
                    UsingList.Add(defUsing);
                }
            }

            return UsingList;
        }

        private static string GetFileRaw(string location)
        {
            string sourceContent;
            location = location.TrimEx();
            if (location.ContainsAny("http://", "https://"))
            {
                PrintLnC($"{location} Downloading...",ConsoleColor.White);
                sourceContent = Net.GetTextFileAsync(location).Result;
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
