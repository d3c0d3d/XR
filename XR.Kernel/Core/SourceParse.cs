using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using XR.Kernel.Extensions;
using XR.Kernel.Std;

namespace XR.Kernel.Core
{
    internal static class SourceParse
    {
        internal static string ParseFile(string file)
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

            var getClasses = CompilerService.GetClassesContent(cleanupFile);
            foreach (var classeContent in getClasses)
            {
                cleanupFile = cleanupFile.Replace(classeContent, null);

                codeBuilder.AppendLine(classeContent);
            }

            var methodsExtracts = Regex.Matches(cleanupFile, Statics.RegexMethodCodeBlock);
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
            var matchs = Regex.Matches(file, Statics.RegexImportFrom);
            if (matchs == null || matchs.Count == 0)
                return file;

            foreach (var match in matchs)
            {
                var url = match.ToString().Replace($"{Statics.ImportFromDefName}(", string.Empty)
                    .Replace("\"", string.Empty)
                    .Replace("(", string.Empty)
                    .Replace(");", string.Empty);

                var code = GetFileRaw(url);

                file = file.Replace(Regex.Match(file, Statics.RegexImportFrom).Value, code);
            }

            return file;
        }

        internal static string GetFileRaw(string location)
        {
            string sourceContent;
            location = location.TrimEx();
            if (location.ContainsAny("http://", "https://"))
            {
                Cli.PrintLnC($"{location} Downloading...", ConsoleColor.White);
                sourceContent = Net.GetTextFileAsync(location).Result;

                if (sourceContent.Contains("<!DOCTYPE html>"))
                    throw new KernelException("Html file is not supported");
            }
            else
            {
                if (!location.EndsWith(".xr"))
                    throw new KernelException("extension (.xr) mandatory");

                sourceContent = File.ReadAllText(location);
            }

            return sourceContent;
        }

        private static List<string> GetUsingsFromFile(string file)
        {
            List<string> UsingList = new List<string>();

            var matchs = Regex.Matches(file, Statics.RegexUsings);

            foreach (var match in matchs)
            {
                UsingList.Add(match.ToString());
            }
            // add default usings from template
            foreach (var defUsing in Templates.DefaultUsings)
            {
                if (!UsingList.Contains(defUsing))
                {
                    UsingList.Add(defUsing);
                }
            }

            return UsingList;
        }
    }
}
