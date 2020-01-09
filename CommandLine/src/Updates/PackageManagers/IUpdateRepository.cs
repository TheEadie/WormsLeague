using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Worms.Updates.PackageManagers
{
    public interface IUpdateRepository
    {
        Task<IEnumerable<Version>> GetAvailableVersions(string id);
        Task DownloadVersion(string id, Version version, string downloadToFolderPath);
    }
}