namespace borealis_flowers.api.Features.Images;

public static class ImagesHandler
{
    public static Func<string, IImagesProvider, Task<IResult>> CheckImageExists()
    {
        return async (string imageName, IImagesProvider fileShare) =>
        {
            var isExist = await fileShare.ExistAsync(imageName);
            return isExist ? Results.Ok() : Results.NotFound();
        };
    }

    public static Func<IImagesProvider, Task<IResult>> GetAllImages()
    {
        return async (IImagesProvider fileShare) =>
        {
            var imageNames = await fileShare.GetImagesAsync();
            return Results.Ok(imageNames);
        };
    }

    public static Func<string, IImagesProvider, Task<IResult>> GetImageFile()
    {
        return async (string imageName, IImagesProvider fileShare) =>
        {
            var imageStream = await fileShare.DownloadImageAsStreamAsync(imageName);
            var mimeType = "image/jpeg";
            return Results.File(imageStream, contentType: mimeType);
        };
    }

    public static Func<string, IImagesProvider, Task<IResult>> GetImageBase64()
    {
        return async (string imageName, IImagesProvider fileShare) =>
        {
            var image = await fileShare.DownloadImageAsBase64Async(imageName);
            return Results.Ok(new { image });
        };
    }

    public static Func<IFormFile, IImagesProvider, HttpContext, Task<IResult>> UploadImage()
    {
        return async (IFormFile file, IImagesProvider fileShare, HttpContext context) =>
        {
            var imageName = await fileShare.UploadImageAsync(file);
            return Results.Created(new Uri($"{context.Request.Path}/{imageName}", UriKind.Relative), new { imageName });
        };
    }

    public static Func<string, IImagesProvider, Task<IResult>> DeleteImage()
    {
        return async (string imageName, IImagesProvider fileShare) =>
        {
            await fileShare.DeleteImageAsync(imageName);
            return Results.NoContent();
        };
    }
}
