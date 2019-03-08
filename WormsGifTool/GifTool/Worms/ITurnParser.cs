using System.Text;

namespace GifTool.Worms
{
    internal interface ITurnParser
    { 
        Turn[] ParseTurns(string turnFileContents);
    }
}