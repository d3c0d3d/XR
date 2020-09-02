using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.RuntimeInformation;

namespace XR.Kernel.Std
{
    public static class OSRuntime
    {
        public static bool IsLinux() => IsOSPlatform(OSPlatform.Linux);
        public static bool IsWindows() => IsOSPlatform(OSPlatform.Windows);
        public static bool IsOSX() => IsOSPlatform(OSPlatform.OSX);
    }
}
