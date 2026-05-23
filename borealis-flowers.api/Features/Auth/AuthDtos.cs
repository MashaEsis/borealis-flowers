namespace borealis_flowers.api.Features.Auth;

public sealed class RegisterRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Phone { get; set; }
}

public sealed class LoginRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

public sealed class AuthResponse
{
    public string Token { get; set; } = "";
    public UserMeDto User { get; set; } = null!;
}

public sealed class UserMeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime? Birthday { get; set; }
    public string Role { get; set; } = "";
    public bool IsAdmin { get; set; }
    public bool IsSpecialist { get; set; }
    public Guid? SpecialistId { get; set; }
    public double WalletBalance { get; set; }
    public int LoyaltyProgress { get; set; }
    public int LoyaltyTarget { get; set; } = 5;
    public bool NextOrderDiscount { get; set; }
}

public sealed class UpdateProfileRequest
{
    public string Name { get; set; } = "";
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime? Birthday { get; set; }
}
