using Azure;
using Azure.Storage.Files.Shares;
using borealis_flowers.api.Infrastructure.Exceptions;
using borealis_flowers.api.Models;
using Microsoft.Extensions.Options;

namespace borealis_flowers.api.Features.Images;
public interface IImagesProvider
{
    Task<string> UploadImageAsync(IFormFile imageDto);
    Task UploadImageFromStreamAsync(Stream imageStream, string imageName);
    Task<Stream> DownloadImageAsStreamAsync(string imageName);
    Task<string> DownloadImageAsBase64Async(string imageName);
    Task<IEnumerable<string>> GetImagesAsync();
    Task DeleteImageAsync(string imageName);
    Task<bool> ExistAsync(string imageName);
}
public class AzureShareUserImagesProvider(
    ShareClient client,
    IImageProcessingService imageProcessingService,
    ILogger<AzureShareUserImagesProvider> logger,
    ICacheService cacheService,
    IOptions<AzureStorageSettings> settings)
    : IImagesProvider
{
    public async Task<bool> ExistAsync(string imageName)
    {
        logger.LogInformation("Executing service method {methodName}", nameof(ExistAsync));

        var dirClient = client.GetDirectoryClient(settings.Value.UserImagesDirectory);
        var file = dirClient.GetFileClient(imageName);
        var exists = await file.ExistsAsync();

        logger.LogInformation("Executed service method {methodName}", nameof(ExistAsync));

        return exists;
    }

    public async Task<string> UploadImageAsync(IFormFile file)
    {
        var image = file;
        var imageGuid = Guid.NewGuid();
        var ext = image.FileName.Split(".").Last();
        var baseImageName = $"{imageGuid}.{ext}";
        var shortImageName = $"{imageGuid}_short.{ext}";

        await UploadImage(image.OpenReadStream(), baseImageName);
        await UploadImage(imageProcessingService.ResizeImage(image.OpenReadStream()), shortImageName);

        return baseImageName;

        async Task UploadImage(Stream content, string imageName)
        {
            logger.LogInformation("Executing service method {methodName} with {data}", nameof(UploadImageAsync),
                new { imageName });

            var directory = client.GetDirectoryClient(settings.Value.UserImagesDirectory);
            var file = directory.GetFileClient(imageName);

            await file.CreateAsync(content.Length);
            await file.UploadRangeAsync(new HttpRange(0, content.Length), content);

            logger.LogInformation("Executed service method {methodName} with {data}", nameof(UploadImageAsync),
                new { imageName });
        }
    }

    public async Task<Stream> DownloadImageAsStreamAsync(string imageName)
    {
        logger.LogInformation("Executing service method {methodName} with {data}", nameof(DownloadImageAsStreamAsync),
            new { imageName });

        var directory = client.GetDirectoryClient(settings.Value.UserImagesDirectory);
        var file = directory.GetFileClient(imageName);
        var isExists = await file.ExistsAsync();

        if (!isExists)
        {
            throw new EntityNotFoundException("Image", imageName);
        }

        var download = await file.DownloadAsync();
        var memoryStream = new MemoryStream();

        await download.Value.Content.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        logger.LogInformation("Executed service method {methodName} with {data}", nameof(DownloadImageAsStreamAsync),
            new { imageName });

        return memoryStream;
    }

    public async Task<string> DownloadImageAsBase64Async(string imageName)
    {
        logger.LogInformation("Executing service method {methodName} with {data}", nameof(DownloadImageAsBase64Async),
            new { imageName });

        logger.LogInformation("Searching {data} in cache", new { imageName });

        if (cacheService.Get<string>(imageName, out var imageBase64) && imageBase64 is not null)
        {
            logger.LogInformation("Searched {data} in cache - Valid", new { imageName });
            logger.LogInformation("Executed service method {methodName} with {data}",
                nameof(DownloadImageAsBase64Async), new { imageName });

            return imageBase64;
        }

        logger.LogInformation("Searched {data} in cache - Invalid", new { imageName });

        var directory = client.GetDirectoryClient(settings.Value.UserImagesDirectory);
        var file = directory.GetFileClient(imageName);
        var isExists = await file.ExistsAsync();

        if (!isExists)
        {
            throw new EntityNotFoundException("Image", imageName);
        }

        var download = await file.DownloadAsync();
        var memoryStream = new MemoryStream();

        await download.Value.Content.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        var bytes = memoryStream.ToArray();

        logger.LogInformation("Saving {data} in cache", new { imageName });

        var base64 = Convert.ToBase64String(bytes);

        cacheService.Set(imageName, base64);

        logger.LogInformation("Saved {data} in cache", new { imageName });

        logger.LogInformation("Executed service method {methodName} with {data}", nameof(DownloadImageAsBase64Async),
            new { imageName });

        return base64;
    }

    public async Task DeleteImageAsync(string imageName)
    {
        logger.LogInformation("Executing service method {methodName} with {data}", nameof(DeleteImageAsync),
            new { imageName });

        logger.LogInformation("Searching {data} in cache for deletion", new { imageName });

        if (cacheService.Get<string>(imageName, out var imageBase64) && imageBase64 is not null)
        {
            logger.LogInformation("Searched {data} in cache for deletion - Valid", new { imageName });
            logger.LogInformation("Deleting {data} from cache", new { imageName });

            cacheService.Remove(imageName);

            logger.LogInformation("Deleted {data} from cache", new { imageName });
        }

        var dirClient = client.GetDirectoryClient(settings.Value.UserImagesDirectory);
        var file = dirClient.GetFileClient(imageName);
        var isExists = await file.ExistsAsync();

        if (!isExists)
        {
            throw new EntityNotFoundException("Image", imageName);
        }

        await file.DeleteAsync();

        logger.LogInformation("Executed service method {methodName} with {data}", nameof(DeleteImageAsync),
            new { imageName });
    }

    public async Task<IEnumerable<string>> GetImagesAsync()
    {
        logger.LogInformation("Executing service method {methodName}", nameof(GetImagesAsync));

        var dirClient = client.GetDirectoryClient(settings.Value.UserImagesDirectory);
        var fileNames = new List<string>();

        await foreach (var file in dirClient.GetFilesAndDirectoriesAsync())
        {
            if (!file.IsDirectory)
            {
                fileNames.Add(file.Name);
            }
        }

        logger.LogInformation("Executed service method {methodName}", nameof(GetImagesAsync));

        return fileNames;
    }

    public async Task UploadImageFromStreamAsync(Stream imageStream, string imageName)
    {
        var nameParts = imageName.Split(".");
        var shortImageName = $"{nameParts[0]}_short.{nameParts[1]}";

        var shortStream = new MemoryStream();
        imageStream.CopyTo(shortStream);

        await UploadImage(imageStream, imageName);
        await UploadImage(imageProcessingService.ResizeImage(shortStream), shortImageName);

        return;

        async Task UploadImage(Stream content, string imageName)
        {
            content.Position = 0;

            logger.LogInformation("Executing service method {methodName} with {data}", nameof(UploadImageAsync),
                new { imageName });

            var directory = client.GetDirectoryClient(settings.Value.UserImagesDirectory);
            var file = directory.GetFileClient(imageName);

            if ((await file.ExistsAsync()).Value)
            {
                await file.DeleteAsync();
                logger.LogInformation("Deleted existing file: {FileName}", imageName);
            }

            await file.CreateAsync(content.Length);
            await file.UploadRangeAsync(new HttpRange(0, content.Length), content);

            logger.LogInformation("Executed service method {methodName} with {data}", nameof(UploadImageAsync),
                new { imageName });
        }
    }
}
