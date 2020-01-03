using System;

namespace Worms.Components.Updates
{
    public interface IComponentUpdater
    {
        void Install(string installFrom, string installTo);
    }
}