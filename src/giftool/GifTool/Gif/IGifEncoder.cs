using System.Collections.Generic;
using System.Threading.Tasks;

namespace GifTool.Gif
{
    internal interface IGifEncoder
    {
        Task CreateGifFromFiles(IEnumerable<string> imageFiles, string outputFile, int animationDelay, int width, int height);
    }
}