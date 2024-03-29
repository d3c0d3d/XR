﻿using System;
using XR.Kernel.Core;
using XR.Std.Logging;
using XR.Std.OptionCommand;
using XR.Std;
using static XR.Std.Cli;

namespace XR.App
{
    class Program
    {
        public static readonly Logger _logger = LoggerFactory.CreateLogger(LogLevel.Info, Util.GetEnvLoggerFile(Kernel.Settings.XR_LOGGER_ENV));

        static void Main(string[] args)
        {
            _logger.Info("Start Runtime");

            Start(args);

            _logger.Info("End Runtime");

        }
        private static void Start(string[] args)
        {
            SetTitle($"{Common.KIND_APPNAME} - {Common.KIND_VERSION}");

            if (args?.Length > 0)
            {
                try
                {
                    OptionCommands.Execute(args);
                }
                catch (OptionException e)
                {
                    PrintErrorMessage(e);
                }
                catch (KernelException e)
                {
                    PrintErrorMessage(e);

                    var error = Util.GetFullError(e);
                    var stack = Util.GetFullStackTraceError(e);
                    var msg = $"{error}{stack}";

                    _logger.Error(msg);
                }
                catch (System.IO.FileNotFoundException e)
                {
                    PrintErrorMessage(e);

                    var error = Util.GetFullError(e);
                    var stack = Util.GetFullStackTraceError(e);
                    var msg = $"{error}{stack}";

                    _logger.Error(msg);
                }
                catch (Exception e)
                {
                    PrintError(e);
                    var error = Util.GetFullError(e);
                    var stack = Util.GetFullStackTraceError(e);
                    var msg = $"{error}{stack}";

                    _logger.Error(msg);
                }
                return;
            }

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
                catch (KernelException e)
                {
                    PrintErrorMessage(e);

                    var error = Util.GetFullError(e);
                    var stack = Util.GetFullStackTraceError(e);
                    var msg = $"{error}{stack}";

                    _logger.Error(msg);
                }
                catch (System.IO.FileNotFoundException e)
                {
                    PrintErrorMessage(e);
                    var error = Util.GetFullError(e);
                    var stack = Util.GetFullStackTraceError(e);
                    var msg = $"{error}{stack}";

                    _logger.Error(msg);
                }
                catch (Exception e)
                {
                    PrintError(e);
                    var error = Util.GetFullError(e);
                    var stack = Util.GetFullStackTraceError(e);
                    var msg = $"{error}{stack}";

                    _logger.Error(msg);
                }
            }
        }
    }
}
