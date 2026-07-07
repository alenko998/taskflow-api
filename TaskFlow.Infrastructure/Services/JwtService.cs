using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(User user, string workspaceId, string role)
    {
        var key     = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:Secret"]!));
        var creds   = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddDays(int.Parse(_config["JwtSettings:ExpiryDays"]!));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email,          user.Email!),
            new Claim(ClaimTypes.GivenName,      user.FirstName),
            new Claim(ClaimTypes.Surname,        user.LastName),
            new Claim(ClaimTypes.Role,           role),
            new Claim("workspaceId",             workspaceId),
        };

        var token = new JwtSecurityToken(
            issuer:             _config["JwtSettings:Issuer"],
            audience:           _config["JwtSettings:Audience"],
            claims:             claims,
            expires:            expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}