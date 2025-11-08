using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using QRCoder;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TreasureHuntApp.Core.Entities;
using TreasureHuntApp.Infrastructure.Data;
using TreasureHuntApp.Shared.DTOs.Authentication;

namespace TreasureHuntApp.Infrastructure.Services;
public class AuthenticationService(TreasureHuntDbContext context, IConfiguration configuration) : IAuthenticationService
{
    private string? JwtSecret => configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Secret not configured");
    private string? JwtIssuer => configuration["Jwt:Issuer"] ?? "TreasureHuntApp";

    public async Task<AdminLoginResponse?> LoginAdminAsync(string username, string password)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.UserName == username);

        if (user == null)
        {
            return null;
        }

        var passwordHasher = new Microsoft.AspNetCore.Identity.PasswordHasher<UserEntity>();
        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash ?? "", password);

        if (result != Microsoft.AspNetCore.Identity.PasswordVerificationResult.Success)
        {
            return null;
        }

        var token = GenerateJwtToken(user);
        var expiresAt = DateTime.UtcNow.AddHours(8);

        return new AdminLoginResponse
        {
            Token = token,
            Username = user.UserName ?? "",
            ExpiresAt = expiresAt
        };
    }

    public async Task<TeamJoinResponse?> JoinTeamAsync(string teamCode, string? teamName = null)
    {
        var team = await context.Teams
            .Include(t => t.Event)
            .FirstOrDefaultAsync(t => t.AccessCode == teamCode.ToUpper());

        if (team == null)
        {
            return null;
        }

        var now = DateTime.UtcNow;
        if (team.Event.StartTime.AddDays(-7) > now || team.Event.EndTime.AddDays(2) < now)
        {
            return null;
        }

        if (!string.IsNullOrEmpty(teamName) && string.IsNullOrEmpty(team.Name))
        {
            team.Name = teamName;
            await context.SaveChangesAsync();
        }

        var sessionToken = Guid.NewGuid().ToString("N");
        var session = new TeamSessionEntity()
        {
            TeamId = team.Id,
            SessionToken = sessionToken,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(12),
            IsActive = true
        };

        context.TeamSessions.Add(session);
        await context.SaveChangesAsync();

        return new TeamJoinResponse
        {
            SessionToken = sessionToken,
            TeamId = team.Id,
            TeamName = team.Name,
            EventId = team.EventId,
            EventName = team.Event.Name
        };
    }

    public async Task<bool> ValidateAdminTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(JwtSecret ?? "");

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = JwtIssuer,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out var validatedToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ValidateTeamSessionAsync(string sessionToken)
    {
        var session = await context.TeamSessions
            .FirstOrDefaultAsync(s => s.SessionToken == sessionToken && s.IsActive);

        return session != null && session.ExpiresAt > DateTime.UtcNow;
    }

    public async Task<TeamEntity?> GetTeamFromSessionAsync(string sessionToken)
    {
        var session = await context.TeamSessions
            .Include(s => s.Team)
            .ThenInclude(t => t.Event)
            .FirstOrDefaultAsync(s => s.SessionToken == sessionToken && s.IsActive);

        return session == null || session.ExpiresAt <= DateTime.UtcNow ? null : session.Team;
    }

    public string GenerateTeamCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public async Task<QrCodeResponse> GenerateTeamQrCodeAsync(string teamCode, string baseUrl)
    {
        var joinUrl = $"{baseUrl.TrimEnd('/')}/join/{teamCode}";

        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(joinUrl, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new Base64QRCode(qrCodeData);

        var qrCodeImage = qrCode.GetGraphic(20);

        return new QrCodeResponse
        {
            QrCodeBase64 = qrCodeImage,
            JoinUrl = joinUrl,
            TeamCode = teamCode
        };
    }

    private string GenerateJwtToken(UserEntity user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(JwtSecret);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.UserName ?? ""),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("role", "admin")
            }),
            Expires = DateTime.UtcNow.AddHours(8),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature),
            Issuer = JwtIssuer
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
