using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WormsScheme
{
    class Program
    {
        static void Main(string[] args)
        {
            var deserialiser = new WscReader(@"C:\Program Files (x86)\Steam\steamapps\common\Worms Armageddon\User\Schemes\{{01}} Beginner.wsc");
            var model = deserialiser.GetModel();

            Console.WriteLine(model.Signature);
            Console.WriteLine(model.Version);
            Console.WriteLine(model.HotSeatDelay);
            Console.WriteLine(model.RetreatTime);
            Console.WriteLine(model.RopeRetreatTime);
            Console.WriteLine(model.HealthCrateProbability);
            Console.WriteLine(model.SheepHeaven);

            Console.ReadKey();
        }
    }
}
