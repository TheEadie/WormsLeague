using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Worms.Armageddon.Game.Win;

public static class WindowTools
{
    [SuppressMessage("Roslynator", "RCS1163:Unused parameter.", Justification = "Required for P/Invoke.")]
    public static IntPtr[] GetWindowsWithTitleMatching(Regex pattern)
    {
        var results = new List<IntPtr>();

        _ = EnumWindows(VisitWindow, IntPtr.Zero);

        return [.. results];

        bool VisitWindow(IntPtr hWnd, IntPtr lParam)
        {
            var length = GetWindowTextLength(hWnd);
            if (length == 0)
            {
                return true;
            }

            var builder = new char[length + 1];
            _ = GetWindowText(hWnd, builder, builder.Length);
            if (pattern.IsMatch(builder.ToString()!))
            {
                results.Add(hWnd);
            }

            return true;
        }
    }

    public static Process? GetProcessForWindow(IntPtr hWnd)
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
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern int GetWindowText(IntPtr hWnd, char[] lpString, int nMaxCount);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern IntPtr SetFocus(IntPtr hWnd);
}
