﻿using System;

namespace XR.Std.OptionCommand
{
    public class OptionException : Exception
    {
        public OptionException(string message, string optionName)
          : base(message)
        {
            OptionName = optionName;
        }

        public OptionException(string message, string optionName, Exception innerException)
          : base(message, innerException)
        {
            OptionName = optionName;
        }

        public string OptionName { get; }
    }
}
