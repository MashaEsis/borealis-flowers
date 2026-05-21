using Microsoft.JSInterop;

namespace borealis_flowers.ui.Services;

/// <summary>Обёртка над localStorage (после OnAfterRender с firstRender).</summary>
public sealed class BrowserStorageService(IJSRuntime js)
{
    public const string StateKey = "borealis-flowers-studio-v1";

    public ValueTask<string?> GetRawAsync(string key) =>
        js.InvokeAsync<string?>("borealisLs.get", key);

    public ValueTask SetRawAsync(string key, string value) =>
        js.InvokeVoidAsync("borealisLs.set", key, value);

    public ValueTask RemoveAsync(string key) =>
        js.InvokeVoidAsync("borealisLs.remove", key);
}
