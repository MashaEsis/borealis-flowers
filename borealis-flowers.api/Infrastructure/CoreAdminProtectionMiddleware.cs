using System.Net.Http.Headers;
using System.Text;

namespace borealis_flowers.api.Infrastructure
{
    public class CoreAdminProtectionMiddleware
    {
        private readonly RequestDelegate _next;

        public CoreAdminProtectionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (environment == Environments.Development && context.Request.Path.StartsWithSegments("/coreadmin"))
            {
                var encodedCredentials = context.Request.Headers.Authorization;
                if (encodedCredentials.ToString().StartsWith("Basic"))
                {
                    string encodedUsernamePassword = encodedCredentials.ToString().Substring("Basic ".Length).Trim();
                    Encoding encoding = Encoding.GetEncoding("iso-8859-1");
                    string usernamePassword = encoding.GetString(Convert.FromBase64String(encodedUsernamePassword));

                    if (usernamePassword == $"sa:admin")
                    {
                        await _next.Invoke(context).ConfigureAwait(false);
                        return;
                    }
                }

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Access to coreadmin\"";
                return;
            }
            await _next.Invoke(context).ConfigureAwait(false);
        }
    }

}
