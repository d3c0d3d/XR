using System;
using XR.Std;

namespace XR.App
{
    internal class Common
    {
        internal const string KIND_APPNAME = "XR - Powered by 0xd3c0d3d";
        internal const string KIND_VERSION = "v0.06-Alpha";

        internal static void PrintBrand()
        {
            var _brand = $@"
'##::::'##:'#########:
. ##::'##:: ##.... ##:
:. ##'##::: ##:::: ##:
::. ###:::: #########:
:: ## ##::: ##.. ##:::
: ##:. ##:: ##::. ##::
 ##:::. ##: ##:::. ##:
..:::::..::..:::::..::
{KIND_VERSION}

© {DateTime.Now.Year} - Powered by 0xd3c0d3d
";
            Cli.PrintLnC(_brand, ConsoleColor.Gray);
        }

        

    }
}
