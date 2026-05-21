namespace borealis_flowers.api.Helpers;

using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;

public static class HttpClientUtils
{
    public static async Task SetTokenAsync(this HttpClient client, HttpContext context)
    {
        var token = await context.GetTokenAsync("access_token");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public static async Task SetTokenAsync(this HttpClient client, IHttpTokenAccessor tokenAccessor)
    {
        var token = await tokenAccessor.GetBearerToken();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}
