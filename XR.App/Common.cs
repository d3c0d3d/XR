using System;
using XR.Core.Util;

namespace XR.App
{
    internal class Common
    {
        internal const string KIND_APPNAME = "XR - Coded by 0xd3c0d3d";
        internal const string KIND_VERSION = "v0.01-Alpha";

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

© {DateTime.Now.Year} - Coded by 0xd3c0d3d
";
            ConsoleHelpers.PrintLnC(_brand, ConsoleColor.Gray);
        }
    }
}
