using System.Diagnostics;
using Serilog;

namespace Worms.Cli.Resources.Local.Folders;

public class WindowsFolderOpener : IFolderOpener
{
    private readonly ILogger _logger;

    public WindowsFolderOpener(ILogger logger)
    {
        _logger = logger;
    }

    public void OpenFolder(string folderPath)
    {
        if (Directory.Exists(folderPath))
        {
            Process.Start("explorer.exe", folderPath);
        }
        else
        {
            _logger.Warning($"Folder {folderPath} does not exist");
        }
    }
}
