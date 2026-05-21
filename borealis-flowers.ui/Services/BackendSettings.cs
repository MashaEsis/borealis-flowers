using System.Text.Json;

namespace borealis_flowers.ui.Services;

public sealed class BackendSettings
{
    public string BackendApiUrl { get; init; } = "";

    /// <summary>Загрузка из wwwroot/appsettings.json (локальный файл сборки Wasm).</summary>
    public static async Task<BackendSettings> LoadAsync(HttpClient fallbackHttp)
    {
        try
        {
            await using Stream stream =
                await fallbackHttp.GetStreamAsync("appsettings.json");
            return await JsonSerializer.DeserializeAsync<BackendSettings>(stream)
                   ?? new BackendSettings();
        }
        catch
        {
            return new BackendSettings();
        }
    }
}
