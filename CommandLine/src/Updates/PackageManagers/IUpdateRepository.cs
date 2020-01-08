using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Worms.Updates.Repositories
{
    public interface IUpdateRepository
    {
        Task<IEnumerable<Version>> GetAvailibleVersions(string id);
        Task DownloadVersion(string id, Version version, string downloadToFolderPath);
    }
}