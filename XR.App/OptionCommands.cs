using System;
using System.Collections.Generic;
using XR.Kernel.Extensions;
using XR.Kernel.OptionCommand;
using XR.Std;

namespace XR.App
{
    internal static class OptionCommands
    {
        private static OptionSet _optionSet;
        internal static string _titleCache;
        private static string _fileRun;
        private static string[] _assembliesLocation;
        private static string[] _assemblyArgs;

        internal static void Startup()
        {
            _titleCache = Cli.GetTitle();
        }

        internal static void Execute(string[] args)
        {
            if (_optionSet == null)
                CreateCommands();
            
            var ret = _optionSet.Parse(args);
            if (ret.Count > 0)
            {
                Main.ShowHelp(_optionSet);
            }
            else
            {
                if (!_fileRun.IsNull())
                {
                    Main.RunFile(_fileRun, _assemblyArgs, _assembliesLocation);
                    _fileRun = null;
                }
            }
        }

        internal static void Idle()
        {
            Cli.ShellCaret(ConsoleColor.White);
        }

        private static void CreateCommands()
        {
            _optionSet = new OptionSet()
                .Add("run=", "Download/Compile and Run {File/Url}", (p) => _fileRun = p)
                .Add("args=", "Set Main {Arg} to Assembly", (p) => {
                    _assemblyArgs = p.Split(new[] { '|' });
                })
                .Add("dep=", "Set Assembly Dependencie {Location}", (p) => {
                    _assembliesLocation = p.Split(new[] { '|' });
                })                
                .Add("cls", "Clear Screen", _ =>
                {
                    Console.Clear();
                })
                .Add("exit", "Exit Application", _ => Main.Exit())
                .Add("?", "Show All Commands", _ => Main.ShowHelp(_optionSet));
        }
    }
}
