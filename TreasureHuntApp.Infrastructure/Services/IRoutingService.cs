using TreasureHuntApp.Shared.DTOs.Routes;

namespace TreasureHuntApp.Infrastructure.Services;
public interface IRoutingService
{
    Task<RouteResponseDto> GetRouteAsync(RouteRequestDto request);
    Task<OptimizedRouteResponseDto> GetOptimizedRouteAsync(OptimizedRouteRequestDto request);
    Task<bool> IsOsrmAvailableAsync(TransportMode mode);
}
