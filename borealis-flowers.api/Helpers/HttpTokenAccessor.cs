using Microsoft.AspNetCore.Authentication;

namespace borealis_flowers.api.Helpers;

public class HttpTokenAccessor : IHttpTokenAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpTokenAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string?> GetBearerToken()
    {
        string? token = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
        return token;
    }
}
