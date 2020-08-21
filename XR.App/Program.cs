using System;
using XR.Core.OptionCommand;
using static XR.Core.Util.ConsoleHelpers;

namespace XR.App
{
    class Program
    {
        static void Main(string[] args)
        {
            CenterConsole();

            SetTitle($"{Common.KIND_APPNAME} - {Common.KIND_VERSION}");
            Common.PrintBrand();

            OptionCommands.Startup();

            while (true)
            {
                try
                {
                    OptionCommands.Idle();
                    OptionCommands.Execute(ShellArgs());
                }
                catch (OptionException e)
                {
                    PrintErrorMessage(e);
                }                
                catch (System.IO.FileNotFoundException e)
                {
                    PrintErrorMessage(e);
                }
                catch (Exception e)
                {
                    PrintError(e);
                }
            }

        }
    }
}
