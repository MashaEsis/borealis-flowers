using System.Net.Http.Headers;
using borealis_flowers.ui.Services;

namespace borealis_flowers.ui.Infrastructure;

public sealed class AuthTokenHandler(JwtState jwt) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (jwt.BearerToken is { Length: > 0 } token &&
            request.RequestUri is not null)
        {
            string path = request.RequestUri.PathAndQuery;
            if (!path.StartsWith("/auth/login", StringComparison.OrdinalIgnoreCase) &&
                !path.StartsWith("/auth/register", StringComparison.OrdinalIgnoreCase))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        return base.SendAsync(request, cancellationToken);
    }
}
