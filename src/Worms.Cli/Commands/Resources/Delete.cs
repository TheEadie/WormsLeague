using System.CommandLine;

namespace Worms.Commands.Resources
{
    internal class Delete : Command
    {
        public Delete() : base("delete", "Delete a resource")
        {
            AddAlias("rm");
        }
    }
}