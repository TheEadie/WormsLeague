using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Worms.Armageddon.Game.Win;

public static class WindowTools
{
    [SuppressMessage("FalsePositive", "RCS1163", Justification = "Unused params are needed by the win32.dll code")]
    public static IntPtr[] GetWindowsWithTitleMatching(Regex pattern)
    {
        var results = new List<IntPtr>();

        bool VisitWindow(IntPtr hWnd, IntPtr lParam)
        {
            var length = GetWindowTextLength(hWnd);
            if (length == 0)
            {
                return true;
            }

            var builder = new StringBuilder(length + 1);
            _ = GetWindowText(hWnd, builder, builder.Capacity);
            if (pattern.IsMatch(builder.ToString()))
            {
                results.Add(hWnd);
            }

            return true;
        }

        _ = EnumWindows(VisitWindow, IntPtr.Zero);

        return results.ToArray();
    }

    public static Process GetProcessForWindow(IntPtr hWnd)
    {
        if (hWnd == IntPtr.Zero)
        {
            return null;
        }

        _ = GetWindowThreadProcessId(hWnd, out var processId);
        return Process.GetProcessById((int) processId);
    }

    public static void FocusWindow(IntPtr hWnd)
    {
        if (hWnd != IntPtr.Zero)
        {
            _ = SetFocus(hWnd);
        }
    }

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetFocus(IntPtr hWnd);
}
