using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace borealis_flowers.api.Features.Images;

public interface IImageProcessingService
{
    Stream ResizeImage(Stream imgStream, int imgWidth = 512, int imgHeight = 512);
}
public class ImageProcessingService(ILogger<ImageProcessingService> logger) : IImageProcessingService
{
    public Stream ResizeImage(Stream imgStream, int imgWidth = 512, int imgHeight = 512)
    {
        logger.LogInformation("Executing service method {methodName}", nameof(ResizeImage));

        using var image = Image.Load(imgStream);
        image.Mutate(x => x.Resize(imgWidth, imgHeight));

        var imageStream = new MemoryStream();
        image.Save(imageStream, new JpegEncoder());
        imageStream.Position = 0;

        logger.LogInformation("Executed service method {methodName}", nameof(ResizeImage));

        return imageStream;
    }
}
