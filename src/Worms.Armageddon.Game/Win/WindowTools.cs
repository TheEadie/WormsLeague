using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Worms.Armageddon.Game.Win;

public static partial class WindowTools
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

        GetWindowThreadProcessId(hWnd, out var processId);
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

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static partial bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [LibraryImport("user32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static partial int GetWindowText(IntPtr hWnd, [Out] char[] lpString, int nMaxCount);

    [LibraryImport("user32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static partial int GetWindowTextLength(IntPtr hWnd);

    [LibraryImport("user32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static partial uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [LibraryImport("user32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static partial IntPtr SetFocus(IntPtr hWnd);
}
