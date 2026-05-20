using Worms.Cli.Resources.Local.Folders;

namespace Worms.Cli.Tests.Fakes;

internal sealed class RecordingFolderOpener : IFolderOpener
{
    public List<string> OpenedFolders { get; } = [];

    public void OpenFolder(string folderPath) => OpenedFolders.Add(folderPath);
}
