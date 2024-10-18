using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace mct_timer.Models
{

    public class ImgSrvData
    {
        public string api { get; set; }
        public string clientRequestId { get; set; }
        public string requestId { get; set; }
        public string eTag { get; set; }
        public string contentType { get; set; }
        public int contentLength { get; set; }
        public string blobType { get; set; }
        public string accessTier { get; set; }
        public string url { get; set; }
        public string sequencer { get; set; }
        public string validationCode { get; set; }
    }

    public class ImgSrvHelper
    {

        public static async Task<bool> ResizeImageAsync(Image<Rgba32> input, Stream output, ImageSize size)
        {
            var dimensions = imageDimensionsTable[size];
            var maxWidth = dimensions.Item1;
            var maxHeight = dimensions.Item2;

            var ratioX = (double)maxWidth / input.Width;
            var ratioY = (double)maxHeight / input.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(input.Width * ratio);
            var newHeight = (int)(input.Height * ratio);

            input.Mutate(x => x.Resize(newWidth, newHeight));

            await input.SaveAsync(output, new PngEncoder());

            return true;
        }

        public enum ImageSize { Small, Medium }

        private static Dictionary<ImageSize, (int, int)> imageDimensionsTable = new Dictionary<ImageSize, (int, int)>() {
        { ImageSize.Small,      (100, 100) },
        { ImageSize.Medium,     (500, 500) }
    };

    }
}
