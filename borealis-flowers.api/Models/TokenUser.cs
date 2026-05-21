using System.Text.Json.Serialization;

namespace borealis_flowers.api.Models;

public class TokenUser
{
    private string? _fullName;

    public string Id { get; set; }

    [JsonPropertyName("displayName")]
    public string? DisplayName
    {
        get => _fullName;
        set => _fullName ??= value;
    }

    [JsonPropertyName("fullName")]
    public string? FullName
    {
        get => _fullName;
        set => _fullName ??= value;
    }

    public string? Phone { get; set; }
    public bool IsAnonymous { get; set; }

    public string Email { get; set; }

    public List<string> Roles { get; set; } = [];
}
