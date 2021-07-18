namespace XR.Kernel
{
    public static class Settings
    {
        // Env
        public const string XR_LOGGER_ENV = "XR_LOGGER";

        // Regexs
        internal const string ImportFromDefName = "ImportFrom";
        internal const string RegexMethodCodeBlock = @"(\[attribute\]|public\s|private\s|protected\s|static\s|internal\s)(?<signature>[^{]*)(?<body>(?:\{[^}]*\}|//.*\r?\n|""[^""]*""|[\S\s])*?\{(?:\{[^}]*\}|//.*\r?\n|""[^""]*""|[\S\s])*?)\}";
        internal const string RegexImportFrom = @"ImportFrom\(""(([a-zA-Z]:\\[\\\S|*\S]?.*)|([\w+]+\:\/\/)?([\w\d-]+\.)*[\w-]+[\.\:]\w+([\/\?\=\&\#]?[\w-]+)*\/?)""\);";
        internal const string RegexUsings = @"using\s[^(var].*\w+;";
    }
}
