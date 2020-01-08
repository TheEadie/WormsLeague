using System.IO.Abstractions;

namespace Worms.Updates.Installers
{
    public class FileCopierInstaller : IFileCopierInstaller
    {
        private readonly IFileSystem _fileSystem;

        public FileCopierInstaller(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }
        public void Install(string installFrom, string installTo)
        {
            BackupCurrentVersion(installTo);
            InstallNewVersion(installFrom, installTo);
        }

        private void InstallNewVersion(string installFrom, string componentPath)
        {
            MoveFilesInFolder(installFrom, componentPath);
        }

        private void BackupCurrentVersion(string componentPath)
        {
            var backupFolder = _fileSystem.Path.Combine(componentPath, ".backup");

            if (_fileSystem.Directory.Exists(backupFolder))
                {
                _fileSystem.Directory.Delete(backupFolder, true);
            }
            _fileSystem.Directory.CreateDirectory(backupFolder);

            MoveFilesInFolder(componentPath, backupFolder);
        }

        private void MoveFilesInFolder(string currentFolder, string newFolder)
        {
            foreach (var file in _fileSystem.Directory.GetFiles(currentFolder))
            {
                var fileName = _fileSystem.Path.GetFileName(file);
                var backupFilePath = _fileSystem.Path.Combine(newFolder, fileName);
                _fileSystem.File.Move(file, backupFilePath);
            }
        }
    }
}