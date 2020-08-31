using XR.Kernel.Extensions;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
#if Windows
using System.Windows.Forms;
#endif

namespace XR.Kernel.Std
{
    public static class Cli
    {
        public static readonly Action<string> Print = (value) => Console.Write(value);
        public static readonly Action<string, ConsoleColor> PrintC = (value, consoleColor) =>
        {
            Console.ForegroundColor = consoleColor;
            Console.Write(value);
            Console.ResetColor();
        };
        public static readonly Action<Exception> PrintError = (ex) =>
        {
            Console.ForegroundColor = ConsoleColor.Red;
            while (ex is AggregateException && ex.InnerException != null)
            {
                ex = ex.InnerException.InnerException ?? ex.InnerException.InnerException;
            }
            if (ex != null)
                Console.WriteLine(ex);
            Console.ResetColor();
        };
        public static readonly Action<Exception> PrintErrorMessage = (ex) =>
        {
            Console.ForegroundColor = ConsoleColor.Red;
            if (ex != null)
                Console.WriteLine(ex.Message);
            Console.ResetColor();
        };
        public static readonly Action<string> PrintLn = (value) => Console.WriteLine(value);
        public static readonly Action<string, ConsoleColor> PrintLnC = (value, consoleColor) =>
        {
            Console.ForegroundColor = consoleColor;
            Console.WriteLine(value);
            Console.ResetColor();
        };

        public static readonly Func<string, ConsoleColor, string> Input = (value, consoleColor) =>
        {
            PrintC($"{value}: ", consoleColor);

            Console.ForegroundColor = ConsoleColor.White;
            var ret = Console.ReadLine();
            Console.ResetColor();
            return ret;
        };


        public static readonly Func<string, string, ConsoleColor, string> InputDef = (value, defvalue, consoleColor) =>
        {
            PrintC($"{value} (default=", consoleColor);
            PrintC($"{defvalue}", ConsoleColor.Yellow);
            PrintC("): ", consoleColor);

            Console.ForegroundColor = ConsoleColor.White;
            var ret = Console.ReadLine();
            if (ret.IsNull())
                ret = defvalue;
            Console.ResetColor();
            return ret;
        };

        public static void ClearLine()
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Console.CursorTop - 1);
        }
        public static void CenterConsole()
        {
#if Windows
            IntPtr hWin = GetConsoleWindow();            
            GetWindowRect(hWin, out RECT rc);
            Screen scr = Screen.FromPoint(new Point(rc.left, rc.top));
            int x = scr.WorkingArea.Left + (scr.WorkingArea.Width - (rc.right - rc.left)) / 2;
            int y = scr.WorkingArea.Top + (scr.WorkingArea.Height - (rc.bottom - rc.top)) / 2;
            MoveWindow(hWin, x, y, rc.right - rc.left, rc.bottom - rc.top, false);
#endif
        }
        public static void SetTitle(string title, bool showArch = true)
        {
#if Windows	
            Console.Title = title + (showArch ? $"{(Environment.Is64BitProcess ? " (x64) " : " (x86) ")}" : null);
#endif
        }

        public static string GetTitle()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return string.Empty;

            return Console.Title;
        }

        public static void ShellCaret(string concat = null)
        {
            PrintC($"{(!concat.IsNull() ? $"{concat}" : "")}:$ ", ConsoleColor.Cyan);
        }
        public static void ShellCaret(ConsoleColor color, string concat = null)
        {
            PrintC($"{(!concat.IsNull() ? $"{concat}" : "")}:$ ", ConsoleColor.Green);
            Console.ForegroundColor = color;
        }

        public static string[] ShellArgs()
        {
            var ret = Console.ReadLine()?.Split(' ');
            Console.ResetColor();
            return ret;
        }

#if Windows

        // P/Invoke declarations
        private struct RECT { public int left, top, right, bottom; }
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT rc);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hWnd, int x, int y, int w, int h, bool repaint);

#endif
    }
}
