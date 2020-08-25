using System;
using System.Linq;
using XR.Kernel.OptionCommand;
using static XR.Kernel.Std.Cli;

namespace XR.App
{
    class Program
    {
        static void Main(string[] args)
        {
            SetTitle($"{Common.KIND_APPNAME} - {Common.KIND_VERSION}");

            if (args?.Count() > 0)
            {
                try
                {
                    OptionCommands.Execute(args);
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
            else
            {
                CenterConsole();
                
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
}
