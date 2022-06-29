using System;
using System.Threading.Tasks;

namespace Worms.Cli.Resources.Remote;

internal interface IWormsServerApi
{
    Task<T> Get<T>(Uri path);
}