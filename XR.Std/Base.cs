using System;
using XR.Std.Extensions;

namespace XR.Std
{
    public static class Base
    {
        public static void ImportFrom(string location)
        {
            if (location.IsNull())
                throw new ArgumentException(location);
        }

        public static void RefFrom(string location)
        {
            if (location.IsNull())
                throw new ArgumentException(location);
        }
    }
}
