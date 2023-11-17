using System.Diagnostics;
using Serilog;

namespace Worms.Cli.Resources.Local.Folders;

public class WindowsFolderOpener(ILogger logger) : IFolderOpener
{
    public void OpenFolder(string folderPath)
    {
        if (Directory.Exists(folderPath))
        {
            _ = Process.Start("explorer.exe", folderPath);
        }
        else
        {
            logger.Warning($"Folder {folderPath} does not exist");
        }
    }
}
