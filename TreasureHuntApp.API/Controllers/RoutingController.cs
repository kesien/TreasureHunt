using Microsoft.AspNetCore.Mvc;
using TreasureHuntApp.Infrastructure.Services;
using TreasureHuntApp.Shared.DTOs.Routes;

namespace TreasureHuntApp.API.Controllers;
[ApiController]
[Route("api/[controller]")]
public class RoutingController(IRoutingService routingService) : ControllerBase
{
    [HttpPost("route")]
    public async Task<ActionResult<RouteResponseDto>> GetRoute([FromBody] RouteRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await routingService.GetRouteAsync(request);

        return !result.Success ? BadRequest(result) : Ok(result);
    }

    [HttpPost("optimized-route")]
    public async Task<ActionResult<OptimizedRouteResponseDto>> GetOptimizedRoute([FromBody] OptimizedRouteRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await routingService.GetOptimizedRouteAsync(request);

        return !result.Success ? BadRequest(result) : Ok(result);
    }

    [HttpGet("health/{mode}")]
    public async Task<IActionResult> CheckHealth(TransportMode mode)
    {
        var isAvailable = await routingService.IsOsrmAvailableAsync(mode);

        return Ok(new
        {
            Mode = mode.ToString(),
            Available = isAvailable,
            Timestamp = DateTime.UtcNow
        });
    }
}
