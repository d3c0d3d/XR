using System;
using XR.Core.OptionCommand;
using XR.Core.Util;

namespace XR.App
{
    internal static class OptionCommands
    {
        private static OptionSet _optionSet;
        internal static string _titleCache;

        internal static void Startup()
        {
            _titleCache = ConsoleHelpers.GetTitle();
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
            ConsoleHelpers.ShellCaret(ConsoleColor.White);
        }

        private static void CreateCommands()
        {
            _optionSet = new OptionSet()
                .Add("run=", "Run file", Main.RunFile)
                .Add("cls", "Clear Screen", _ =>
                {
                    Console.Clear();
                })
                .Add("exit", "Exit Application/Client Session", _ => Main.Exit())
                .Add("?", "Show All Commands", _ => Main.ShowHelp(_optionSet));
        }
    }
}
