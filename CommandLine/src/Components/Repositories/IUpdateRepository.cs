using System;
using System.Collections.Generic;

namespace Worms.Components.Repositories
{
    public interface IUpdateRepository
    {
        IEnumerable<Version> GetAvailibleVersions(string id);
        void DownloadVersion(string id, Version version, string downloadToFolderPath);
    }
}