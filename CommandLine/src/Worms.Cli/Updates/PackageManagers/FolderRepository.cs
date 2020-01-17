using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;

namespace Worms.Updates.PackageManagers
{
    public class FolderRepository : IUpdateRepository
    {
        private readonly string _repoFolderPath;
        
        private readonly IFileSystem _fileSystem;

        public FolderRepository(string repoFolderPath, IFileSystem fileSystem)
        {
            _repoFolderPath = repoFolderPath;
            _fileSystem = fileSystem;
        }

        public Task<IEnumerable<Version>> GetAvailableVersions(string id)
        {
            var componentFolder = GetComponentFolder(id);

            var folders = _fileSystem.Directory.GetDirectories(componentFolder);
            var folderNames = folders.Select(x => _fileSystem.Path.GetFileName(x));

            var versions = new List<Version>();
            foreach (var folder in folderNames)
            {
                if (Version.TryParse(folder, out var version))
                {
                    versions.Add(version);
                }
            }

            return Task.FromResult((IEnumerable<Version>) versions);
        }

        public Task DownloadVersion(string id, Version version, string downloadToFolderPath)
        {
            var componentPath = GetComponentFolder(id);
            var componentVersionPath = _fileSystem.Path.Combine(componentPath, version.ToString(3));

            var files = _fileSystem.Directory.GetFiles(componentVersionPath);

            foreach (var file in files)
            {
                var fileName = _fileSystem.Path.GetFileName(file);
                var destFile = _fileSystem.Path.Combine(downloadToFolderPath, fileName);
                _fileSystem.File.Copy(file, destFile, true);
            }

            return Task.CompletedTask;
        }

        private string GetComponentFolder(string id)
        {
            var componentFolder = _fileSystem.Path.Combine(_repoFolderPath, id);

            if (!_fileSystem.Directory.Exists(componentFolder))
            {
                throw new ArgumentException($"{id} not found in repository");
            }

            return componentFolder;
        }
    }
}