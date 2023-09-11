using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace Worms.Armageddon.Game.Win
{
    internal class SteamService : ISteamService
    {
        private const string ProcessName = "Steam";

        private static readonly Regex LaunchGamePromptRegex = new Regex(
            "Allow game launch\\?",
            RegexOptions.IgnoreCase);

        public void WaitForSteamPrompt()
        {
            Thread.Sleep(500);
            var promptWindow = GetSteamPromptWindow();
            WindowTools.FocusWindow(promptWindow);

            while (promptWindow != IntPtr.Zero)
            {
                Thread.Sleep(10);
                promptWindow = GetSteamPromptWindow();
            }
        }

        private static IntPtr GetSteamPromptWindow()
        {
            var windows = WindowTools.GetWindowsWithTitleMatching(LaunchGamePromptRegex);
            return Array.Find(windows, w => WindowTools.GetProcessForWindow(w).ProcessName == ProcessName);
        }
    }
}
