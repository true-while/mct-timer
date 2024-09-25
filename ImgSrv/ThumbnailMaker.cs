using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ImgSrv
{
    public class ThumbnailMaker
    {


        [FunctionName("ThumbnailMaker")]
        public static void Run(
                [BlobTrigger("$web/l/{name}.{ext}")] Stream image,
                [Blob("$web/s/{name}.png", FileAccess.Write)] Stream imageSmall,
                [Blob("$web/m/{name}.png", FileAccess.Write)] Stream imageMedium,
                string name, string ext,
                ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name}.{ext} \n Size: {image.Length} Bytes");
            
       
            using (Image<Rgba32> input = Image.Load<Rgba32>(image))
            {
                ResizeImage(input, imageSmall, ImageSize.Small);
            }

            image.Position = 0;
            using (Image<Rgba32> input = Image.Load<Rgba32>(image))
            {
                ResizeImage(input, imageMedium, ImageSize.Medium);
            }


        }


        public static void ResizeImage(Image<Rgba32> input, Stream output, ImageSize size)
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

            input.Save(output, new PngEncoder() );
        }

        public enum ImageSize { Small, Medium }

        private static Dictionary<ImageSize, (int, int)> imageDimensionsTable = new Dictionary<ImageSize, (int, int)>() {
        { ImageSize.Small,      (80, 80) },
        { ImageSize.Medium,     (500, 500) }
    };


    }
}



