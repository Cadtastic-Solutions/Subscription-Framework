using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LicenseService.Interfaces;
using LicenseService.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace LicenseService.Services
{
    /// <summary>
    /// Service for cryptographic operations, primarily focused on JWT token signing for offline license tokens
    /// </summary>
    public class CryptoService : ICryptoService
    {
        private readonly IConfiguration _configuration;
        private readonly string _signingKey;
        private readonly string _issuer;
        private readonly string _audience;

        /// <summary>
        /// Initializes a new instance of the <see cref="CryptoService"/> class
        /// </summary>
        /// <param name="configuration">Application configuration to access cryptographic settings</param>
        public CryptoService(IConfiguration configuration)
        {
            _configuration = configuration;
            // In production, these should be loaded from Key Vault
            _signingKey = _configuration["OfflineToken:SigningKey"] ?? "YourTemporaryKeyForDevelopmentEnvironmentOnly123!";
            _issuer = _configuration["OfflineToken:Issuer"] ?? "LicenseService";
            _audience = _configuration["OfflineToken:Audience"] ?? "LicensedApplications";
        }

        /// <inheritdoc />
        public Task<string> SignTokenAsync(OfflineToken token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_signingKey);
            
            var claims = new[]
            {
                new Claim("licenseId", token.LicenseId),
                new Claim("appGuid", token.AppGuid),
                new Claim("issuedAt", token.IssuedAt.ToString("o")),
                new Claim("expiresAt", token.ExpiresAt.ToString("o")),
                new Claim("features", JsonSerializer.Serialize(token.Features)),
                new Claim("plan", token.Plan ?? string.Empty)
            };

            if (!string.IsNullOrEmpty(token.DeviceId))
            {
                claims = [.. claims, new Claim("deviceId", token.DeviceId)];
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = token.ExpiresAt,
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(securityToken);

            return Task.FromResult(jwt);
        }

        /// <inheritdoc />
        public Task<bool> VerifyTokenSignatureAsync(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_signingKey);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out _);

                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }
    }
} 