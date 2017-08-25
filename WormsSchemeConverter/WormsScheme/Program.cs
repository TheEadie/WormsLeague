using System;
using System.Linq;

namespace WormsScheme
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var fileOne = args[0];
            var fileTwo = args[1];

            Scheme model;
            
            if (fileOne.EndsWith(".wsc"))
            {
                var wscReader = new WscReader(fileOne);
                model = wscReader.GetModel();
            }
            else
            {
                var textFileReader = new TextFileReader(fileOne);
                model = textFileReader.GetModel();
            }

            if (args.Contains("/r"))
            {
                var randomChangesGenerator = new RandomSchemeGenerator();
                model = randomChangesGenerator.GetModel(model);
            }
            
            if (fileTwo.EndsWith(".wsc"))
            {
                var wscCreator = new WscWriter(fileTwo);
                wscCreator.WriteModel(model);
            }
            else
            {
                var textFileCreator = new TextFileWriter(fileTwo);
                textFileCreator.WriteModel(model);
            }
        }
    }
}
