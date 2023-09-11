using System.Diagnostics;
using System.IO;
using Serilog;

namespace Worms.Cli.Resources.Local.Folders;

public class LinuxFolderOpener : IFolderOpener
{
    private readonly ILogger _logger;

    public LinuxFolderOpener(ILogger logger)
    {
        _logger = logger;
    }

    public void OpenFolder(string folderPath)
    {
        if (Directory.Exists(folderPath))
        {
            Process.Start("open", folderPath);
        }
        else
        {
            _logger.Warning($"Folder {folderPath} does not exist");
        }
    }
}
