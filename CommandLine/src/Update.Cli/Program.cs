using System.Diagnostics;
using System.IO;

namespace Update.Cli
{
    internal static class Program
    {
        public static void Main()
        {
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
