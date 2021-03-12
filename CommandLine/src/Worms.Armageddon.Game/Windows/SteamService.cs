using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace Worms.Armageddon.Game.Windows
{
    internal class SteamService : ISteamService
    {
        private const string _processName = "Steam";

        private static readonly Regex _launchGamePromptRegex = new Regex(
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
            var windows = WindowTools.GetWindowsWithTitleMatching(_launchGamePromptRegex);
            return Array.Find(windows, w => WindowTools.GetProcessForWindow(w).ProcessName == _processName);
        }
    }
}
