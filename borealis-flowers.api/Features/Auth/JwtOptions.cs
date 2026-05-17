namespace borealis_flowers.api.Features.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "borealis-flowers";
    public string Audience { get; set; } = "borealis-flowers-ui";
    /// <summary>Минимум 32 символа для HS256.</summary>
    public string Key { get; set; } = "";
    public int ExpireMinutes { get; set; } = 10080; // 7 дней
}
