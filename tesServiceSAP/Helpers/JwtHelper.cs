using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

public static class JwtHelper
{
    private const string SecretKey = "AstungkaraBali-secret-key-1234567890"; // ➕ Simpan ke appsettings.json nanti
    private const int ExpirationMinutes = 60;

    public static string GenerateToken(string userName, string sessionId, string routeId)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, userName),
            new Claim("SapSessionId", sessionId),
            new Claim("RouteId", routeId)
        };

        var token = new JwtSecurityToken(
            issuer: "Sap7Layer",
            audience: "Miranesia",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(ExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));

            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = "Sap7Layer",
                ValidAudience = "Miranesia",
                IssuerSigningKey = securityKey
            };

            var handler = new JwtSecurityTokenHandler();
            return handler.ValidateToken(token, parameters, out _);
        }
        catch
        {
            return null; // Token invalid atau expired
        }
    }
}