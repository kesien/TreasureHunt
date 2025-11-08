using Microsoft.AspNetCore.Mvc;
using TreasureHuntApp.API.Attributes;
using TreasureHuntApp.Infrastructure.Services;
using TreasureHuntApp.Shared.DTOs.Authentication;

namespace TreasureHuntApp.API.Controllers;
[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthenticationService authService) : ControllerBase
{
    [HttpPost("admin/login")]
    public async Task<ActionResult<AdminLoginResponse>> AdminLogin([FromBody] AdminLoginRequest request)
    {
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest("Username and password are required");
        }

        var result = await authService.LoginAdminAsync(request.Username, request.Password);

        return result is null ? Unauthorized("Invalid credentials") : Ok(result);
    }

    [HttpPost("team/join")]
    public async Task<ActionResult<TeamJoinResponse>> TeamJoin([FromBody] TeamJoinRequest request)
    {
        if (string.IsNullOrEmpty(request.TeamCode))
        {
            return BadRequest("Team code is required");
        }

        var result = await authService.JoinTeamAsync(request.TeamCode, request.TeamName);

        return result == null ? (ActionResult<TeamJoinResponse>)BadRequest("Invalid team code or event not active") : (ActionResult<TeamJoinResponse>)Ok(result);
    }

    [HttpGet("team/qr/{teamCode}")]
    [TeamAuthorize]
    public async Task<ActionResult<QrCodeResponse>> GetTeamQrCode(string teamCode)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var result = await authService.GenerateTeamQrCodeAsync(teamCode, baseUrl);

        return Ok(result);
    }

    [HttpPost("validate")]
    public async Task<ActionResult<bool>> ValidateToken([FromBody] string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest("Token is required");
        }

        // Check if it's a JWT token (admin) or session token (team)
        if (token.Contains('.')) // JWT format
        {
            var isValid = await authService.ValidateAdminTokenAsync(token);
            return Ok(isValid);
        }
        else
        {
            var isValid = await authService.ValidateTeamSessionAsync(token);
            return Ok(isValid);
        }
    }
}
