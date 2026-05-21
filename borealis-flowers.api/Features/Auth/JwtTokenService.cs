using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using borealis_flowers.api.Data.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace borealis_flowers.api.Features.Auth;

public sealed class JwtTokenService(IOptions<JwtOptions> options)
{
    readonly JwtOptions _opt = options.Value;

    public string CreateToken(Customer customer)
    {
        string role = ResolveRole(customer);

        Claim[] claims =
        [
            new(JwtRegisteredClaimNames.Sub, customer.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, customer.Email ?? ""),
            new(ClaimTypes.Name, customer.Name),
            new(ClaimTypes.Role, role),
            new("specialistId", customer.SpecialistId?.ToString() ?? ""),
        ];

        byte[] keyBytes = Encoding.UTF8.GetBytes(_opt.Key);
        if (keyBytes.Length < 32)
            throw new InvalidOperationException("Jwt:Key must be at least 32 characters.");

        var securityKey = new SymmetricSecurityKey(keyBytes);
        var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _opt.Issuer,
            audience: _opt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_opt.ExpireMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static string ResolveRole(Customer c) =>
        c.IsAdmin ? "Admin" : c.IsSpecialist ? "Florist" : "Client";
}
