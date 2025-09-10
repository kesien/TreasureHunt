using TreasureHuntApp.Shared.DTOs;

namespace TreasureHuntApp.Infrastructure.Services;
public interface IRoutingService
{
    Task<RouteResponseDto> GetRouteAsync(RouteRequestDto request);
    Task<OptimizedRouteResponseDto> GetOptimizedRouteAsync(OptimizedRouteRequestDto request);
    Task<bool> IsOsrmAvailableAsync(TransportMode mode);
}
