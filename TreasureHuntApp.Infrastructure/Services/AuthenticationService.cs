using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using QRCoder;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TreasureHuntApp.Core.Entities;
using TreasureHuntApp.Infrastructure.Data;
using TreasureHuntApp.Shared.DTOs;

namespace TreasureHuntApp.Infrastructure.Services;
public class AuthenticationService : IAuthenticationService
{
    private readonly TreasureHuntDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly string _jwtSecret;
    private readonly string _jwtIssuer;

    public AuthenticationService(TreasureHuntDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
        _jwtSecret = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Secret not configured");
        _jwtIssuer = _configuration["Jwt:Issuer"] ?? "TreasureHuntApp";
    }

    public async Task<AdminLoginResponse?> LoginAdminAsync(string username, string password)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserName == username);

        if (user == null)
        {
            return null;
        }

        // Use Identity's password hasher for verification
        var passwordHasher = new Microsoft.AspNetCore.Identity.PasswordHasher<UserEntity>();
        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash ?? "", password);

        if (result != Microsoft.AspNetCore.Identity.PasswordVerificationResult.Success)
        {
            return null;
        }

        var token = GenerateJwtToken(user);
        var expiresAt = DateTime.UtcNow.AddHours(8); // 8 hour expiry

        return new AdminLoginResponse
        {
            Token = token,
            Username = user.UserName ?? "",
            ExpiresAt = expiresAt
        };
    }

    public async Task<TeamJoinResponse?> JoinTeamAsync(string teamCode, string? teamName = null)
    {
        var team = await _context.Teams
            .Include(t => t.Event)
            .FirstOrDefaultAsync(t => t.AccessCode == teamCode.ToUpper());

        if (team == null)
        {
            return null;
        }

        // Check if event is active
        var now = DateTime.UtcNow;
        if (team.Event.StartTime > now || team.Event.EndTime < now)
        {
            return null;
        }

        // Update team name if provided
        if (!string.IsNullOrEmpty(teamName) && string.IsNullOrEmpty(team.Name))
        {
            team.Name = teamName;
            await _context.SaveChangesAsync();
        }

        // Create team session
        var sessionToken = Guid.NewGuid().ToString("N");
        var session = new TeamSessionEntity()
        {
            TeamId = team.Id,
            SessionToken = sessionToken,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(12), // 12 hour session
            IsActive = true
        };

        _context.TeamSessions.Add(session);
        await _context.SaveChangesAsync();

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
            var key = Encoding.ASCII.GetBytes(_jwtSecret);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtIssuer,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ValidateTeamSessionAsync(string sessionToken)
    {
        var session = await _context.TeamSessions
            .FirstOrDefaultAsync(s => s.SessionToken == sessionToken && s.IsActive);

        return session != null && session.ExpiresAt > DateTime.UtcNow;
    }

    public async Task<TeamEntity?> GetTeamFromSessionAsync(string sessionToken)
    {
        var session = await _context.TeamSessions
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
        var key = Encoding.ASCII.GetBytes(_jwtSecret);
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
            Issuer = _jwtIssuer
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    Task<TeamEntity?> IAuthenticationService.GetTeamFromSessionAsync(string sessionToken)
    {
        throw new NotImplementedException();
    }
}
