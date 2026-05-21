using borealis_flowers.ui.Infrastructure;
using borealis_flowers.ui.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<borealis_flowers.ui.App>("#app");
builder.RootComponents.Add<Microsoft.AspNetCore.Components.Web.HeadOutlet>("head::after");

builder.Services.AddSingleton<JwtState>();
builder.Services.AddTransient<AuthTokenHandler>();

builder.Services.AddHttpClient<BackendApiClient>((_, client) =>
{
    client.BaseAddress = new Uri("http://localhost:5298/");
    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
}).AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddSingleton<FloristCatalogService>();
builder.Services.AddScoped<AppSessionService>();
builder.Services.AddScoped<BrowserStorageService>();

builder.Services.AddScoped(_ =>
{
    Uri baseUri = new(builder.HostEnvironment.BaseAddress);
    return new HttpClient { BaseAddress = baseUri };
});

await builder.Build().RunAsync();
