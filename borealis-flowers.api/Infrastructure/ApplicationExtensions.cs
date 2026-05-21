using Azure.Storage.Files.Shares;
using borealis_flowers.api.Data;
using borealis_flowers.api.Features.Images;
using borealis_flowers.api.Models;

namespace borealis_flowers.api.Infrastructure;

public static class ApplicationExtensions
{
    public static IServiceCollection AddConfigureOption(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AzureStorageSettings>(configuration.GetSection(nameof(AzureStorageSettings)));
        services.Configure<AzureStorageSettings>(configuration.GetSection(nameof(AzureStorageSettings)));
        services.Configure<FirebaseSettings>(configuration.GetSection(nameof(FirebaseSettings)));

        return services;
    }

    public static IServiceCollection AddAzureConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var azureStorageSettings = configuration.GetSection(nameof(AzureStorageSettings)).Get<AzureStorageSettings>();

        services.AddSingleton<ShareClient>(x => new ShareClient(
            azureStorageSettings?.ConnectionString,
            Path.Combine(azureStorageSettings?.RootDirectory, azureStorageSettings?.UserImagesDirectory)));

        return services;
    }

    public static IServiceCollection AddCache(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddScoped<ICacheService, ImageCacheService>();

        return services;
    }

    public static IServiceCollection AddImageProcessing(this IServiceCollection services)
    {
        services.AddScoped<IImagesProvider, AzureShareUserImagesProvider>();
        services.AddScoped<IImageProcessingService, ImageProcessingService>();

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IFireApiConnector, CoreFireApiConnector>();

        return services;
    }

}
