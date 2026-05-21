namespace borealis_flowers.ui.Services;

/// <summary>Хранит JWT для <see cref="Infrastructure.AuthTokenHandler"/>.</summary>
public sealed class JwtState
{
    public string? BearerToken { get; set; }
}
