using System;
using XR.Kernel.OptionCommand;
using XR.Kernel.Std;

namespace XR.App
{
    internal static class OptionCommands
    {
        private static OptionSet _optionSet;
        internal static string _titleCache;

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
        }

        internal static void Idle()
        {
            Cli.ShellCaret(ConsoleColor.White);
        }

        private static void CreateCommands()
        {
            _optionSet = new OptionSet()
                .Add("run=", "Download/Compile and Run File", Main.RunFile)
                .Add("cls", "Clear Screen", _ =>
                {
                    Console.Clear();
                })
                .Add("exit", "Exit Application", _ => Main.Exit())
                .Add("?", "Show All Commands", _ => Main.ShowHelp(_optionSet));
        }
    }
}
