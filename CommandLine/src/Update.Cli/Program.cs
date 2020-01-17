using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace Update.Cli
{
    internal static class Program
    {
        public static void Main()
        {
            // Make sure worms CLI has exited
            while(Process.GetProcessesByName("worms").Length > 0)
            {
                Console.WriteLine("Waiting");
                Thread.Sleep(500);
            }

            var runningDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName);
            var updateFolder = Path.Combine(runningDirectory, ".update");

            if (Directory.Exists(updateFolder))
            {
                MoveFilesInFolder(updateFolder, runningDirectory);
            }
        }

        private static void MoveFilesInFolder(string currentFolder, string newFolder)
        {
            foreach (var file in Directory.GetFiles(currentFolder))
            {
                var fileName = Path.GetFileName(file);
                if (fileName.StartsWith("update"))
                {
                    continue;
                }

                var newFilePath = Path.Combine(newFolder, fileName);
                File.Move(file, newFilePath, true);
            }
        }
    }
}
