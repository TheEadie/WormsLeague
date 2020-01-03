using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Worms.Components.Repositories
{
    public interface IUpdateRepository
    {
        Task<IEnumerable<Version>> GetAvailibleVersions(string id);
        void DownloadVersion(string id, Version version, string downloadToFolderPath);
    }
}