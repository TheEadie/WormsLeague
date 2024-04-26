using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Worms.Cli.Resources.Local.Folders;

public class WindowsFolderOpener(ILogger<WindowsFolderOpener> logger) : IFolderOpener
{
    public void OpenFolder(string folderPath)
    {
        if (Directory.Exists(folderPath))
        {
            _ = Process.Start("explorer.exe", folderPath);
        }
        else
        {
            logger.LogWarning("Folder {FolderPath} does not exist", folderPath);
        }
    }
}
