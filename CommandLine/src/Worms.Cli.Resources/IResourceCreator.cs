﻿using System.Threading.Tasks;

namespace Worms.Cli.Resources
{
    public interface IResourceCreator<T, in TParams>
    {
        Task<T> Create(TParams parameters);
    }
}
