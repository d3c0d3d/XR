using System;
using XR.Kernel.Std;

namespace XR.App
{
    internal class Common
    {
        internal const string KIND_APPNAME = "XR - Powered by 0xd3c0d3d";
        internal const string KIND_VERSION = "v0.03-Alpha";

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

        // Regexs
        internal const string ImportFromDefName = "ImportFrom";
        internal const string RegexMethodCodeBlock = @"(\[attribute\]|public|private|protected|static)(?<signature>[^{]*)(?<body>(?:\{[^}]*\}|//.*\r?\n|""[^""]*""|[\S\s])*?\{(?:\{[^}]*\}|//.*\r?\n|""[^""]*""|[\S\s])*?)\}";
        internal const string RegexImportFrom = @"ImportFrom\(""(([a-zA-Z]:\\[\\\S|*\S]?.*)|([\w+]+\:\/\/)?([\w\d-]+\.)*[\w-]+[\.\:]\w+([\/\?\=\&\#]?[\w-]+)*\/?)""\);";
        internal const string RegexUsings = @"using\s?.*\w+;";

    }
}
