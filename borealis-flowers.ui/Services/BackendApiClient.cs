namespace borealis_flowers.ui.Services;

public sealed class BackendApiClient
{
    public HttpClient Http { get; }

    public BackendApiClient(HttpClient http) =>
        Http = http;
}
