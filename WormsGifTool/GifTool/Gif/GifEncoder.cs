using System.Collections.Generic;
using System.Threading.Tasks;
using ImageMagick;

namespace GifTool.Gif
{
    internal class GifEncoder : IGifEncoder
    {
        public Task CreateGifFromFiles(IEnumerable<string> imageFiles, string outputFile, int animationDelay, int width, int height)
        {
            return Task.Run(() =>
            {
                using (var collection = new MagickImageCollection())
                {
                    foreach (var file in imageFiles)
                    {
                        var image = new MagickImage(file);
                        image.Resize(width, height);
                        image.AnimationDelay = animationDelay;
                        collection.Add(image);
                    }

                    var settings = new QuantizeSettings {Colors = 256};

                    collection.Quantize(settings);
                    collection.Optimize();
                    collection.Write(outputFile);
                }
            });
        }
    }
}
