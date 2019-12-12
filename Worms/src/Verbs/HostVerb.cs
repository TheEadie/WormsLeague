using System;
using System.Net;

namespace Worms.Verbs
{
    internal class HostVerb
    {
        public HostVerb()
        {
            
        }

        public void Run()
        {
            var request = WebRequest.Create("https://");
        }
    }
}