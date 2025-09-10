using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.Json;
using TreasureHuntApp.Shared.DTOs;

namespace TreasureHuntApp.Infrastructure.Services;

public class RoutingService(HttpClient httpClient, IConfiguration configuration, ILogger<RoutingService> logger)
    : IRoutingService
{
    public async Task<RouteResponseDto> GetRouteAsync(RouteRequestDto request)
    {
        try
        {
            var baseUrl = GetOsrmUrl(request.TransportMode);
            var profile = GetProfile(request.TransportMode);

            var url = $"{baseUrl}/route/v1/{profile}/{request.StartLongitude},{request.StartLatitude};{request.EndLongitude},{request.EndLatitude}?overview=full&geometries=geojson&steps=true";

            logger.LogInformation("OSRM Request: {Url}", url);
            logger.LogInformation("Coordinates: Start({StartLon},{StartLat}) End({EndLon},{EndLat})",
                request.StartLongitude, request.StartLatitude, request.EndLongitude, request.EndLatitude);

            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("OSRM HTTP Error: {StatusCode}", response.StatusCode);
                return new RouteResponseDto
                {
                    Success = false,
                    ErrorMessage = $"OSRM service error: {response.StatusCode}"
                };
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var osrmResponse = JsonSerializer.Deserialize<OsrmRouteResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (osrmResponse?.Code != "Ok" || osrmResponse.Routes?.Count == 0)
            {
                return new RouteResponseDto
                {
                    Success = false,
                    ErrorMessage = osrmResponse?.Message ?? "No route found"
                };
            }

            var route = osrmResponse.Routes[0];

            return new RouteResponseDto
            {
                Success = true,
                Route = new RouteData
                {
                    Distance = route.Distance,
                    Duration = route.Duration,
                    Coordinates = route.Geometry.Coordinates,
                    Steps = route.Legs.SelectMany(leg => leg.Steps.Select(step => new RouteStep
                    {
                        Distance = step.Distance,
                        Duration = step.Duration,
                        Instruction = step.Maneuver.Instruction ?? "",
                        Maneuver = step.Maneuver.Type ?? ""
                    })).ToList()
                }
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting route from OSRM");
            return new RouteResponseDto
            {
                Success = false,
                ErrorMessage = "Internal server error"
            };
        }
    }

    public async Task<OptimizedRouteResponseDto> GetOptimizedRouteAsync(OptimizedRouteRequestDto request)
    {
        try
        {
            if (request.Waypoints.Count == 0)
            {
                return new OptimizedRouteResponseDto
                {
                    Success = false,
                    ErrorMessage = "No waypoints provided"
                };
            }

            // Use OSRM's table service to get distance matrix
            var matrix = await GetDistanceMatrixAsync(request);

            if (!matrix.Success)
            {
                return new OptimizedRouteResponseDto
                {
                    Success = false,
                    ErrorMessage = matrix.ErrorMessage
                };
            }

            // Simple greedy TSP solver (for small numbers of locations)
            var optimalOrder = SolveTSP(matrix.Distances!, request.ReturnToStart);

            // Get detailed routes for each segment
            var segments = new List<RouteSegment>();
            double totalDistance = 0;
            double totalDuration = 0;

            for (int i = 0; i < optimalOrder.Count - 1; i++)
            {
                var fromIndex = optimalOrder[i];
                var toIndex = optimalOrder[i + 1];

                var fromPoint = fromIndex == 0 ?
                    new { Lat = request.StartLatitude, Lon = request.StartLongitude, Id = 0 } :
                    new { Lat = request.Waypoints[fromIndex - 1].Latitude, Lon = request.Waypoints[fromIndex - 1].Longitude, Id = request.Waypoints[fromIndex - 1].LocationId };

                var toPoint = toIndex == 0 ?
                    new { Lat = request.StartLatitude, Lon = request.StartLongitude, Id = 0 } :
                    new { Lat = request.Waypoints[toIndex - 1].Latitude, Lon = request.Waypoints[toIndex - 1].Longitude, Id = request.Waypoints[toIndex - 1].LocationId };

                var segmentRoute = await GetRouteAsync(new RouteRequestDto
                {
                    StartLatitude = fromPoint.Lat,
                    StartLongitude = fromPoint.Lon,
                    EndLatitude = toPoint.Lat,
                    EndLongitude = toPoint.Lon,
                    TransportMode = request.TransportMode
                });

                if (segmentRoute.Success && segmentRoute.Route != null)
                {
                    segments.Add(new RouteSegment
                    {
                        FromLocationId = fromPoint.Id,
                        ToLocationId = toPoint.Id,
                        Distance = segmentRoute.Route.Distance,
                        Duration = segmentRoute.Route.Duration,
                        Coordinates = segmentRoute.Route.Coordinates
                    });

                    totalDistance += segmentRoute.Route.Distance;
                    totalDuration += segmentRoute.Route.Duration;
                }
            }

            return new OptimizedRouteResponseDto
            {
                Success = true,
                OptimalOrder = optimalOrder.Skip(1).Take(request.Waypoints.Count).Select(i => request.Waypoints[i - 1].LocationId).ToList(),
                TotalDistance = totalDistance,
                TotalDuration = totalDuration,
                RouteSegments = segments
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting optimized route");
            return new OptimizedRouteResponseDto
            {
                Success = false,
                ErrorMessage = "Internal server error"
            };
        }
    }

    public async Task<bool> IsOsrmAvailableAsync(TransportMode mode)
    {
        try
        {
            var baseUrl = GetOsrmUrl(mode);
            var response = await httpClient.GetAsync($"{baseUrl}/nearest/v1/driving/0,0", HttpCompletionOption.ResponseHeadersRead);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private async Task<DistanceMatrixResult> GetDistanceMatrixAsync(OptimizedRouteRequestDto request)
    {
        try
        {
            var baseUrl = GetOsrmUrl(request.TransportMode);
            var profile = GetProfile(request.TransportMode);

            // Build coordinate string with forced dot decimal separator
            var coordinates = new List<string>
            {
                $"{request.StartLongitude.ToString("F6", CultureInfo.InvariantCulture)},{request.StartLatitude.ToString("F6", CultureInfo.InvariantCulture)}"
            };

            coordinates.AddRange(request.Waypoints.Select(w =>
                $"{w.Longitude.ToString("F6", CultureInfo.InvariantCulture)},{w.Latitude.ToString("F6", CultureInfo.InvariantCulture)}"));

            if (request.ReturnToStart)
            {
                coordinates.Add($"{request.StartLongitude.ToString("F6", CultureInfo.InvariantCulture)},{request.StartLatitude.ToString("F6", CultureInfo.InvariantCulture)}");
            }

            var coordinateString = string.Join(";", coordinates);
            var url = $"{baseUrl}/table/v1/{profile}/{coordinateString}";

            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return new DistanceMatrixResult { Success = false, ErrorMessage = $"HTTP {response.StatusCode}" };
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var osrmResponse = JsonSerializer.Deserialize<OsrmTableResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return osrmResponse?.Code != "Ok"
                ? new DistanceMatrixResult { Success = false, ErrorMessage = osrmResponse?.Message ?? "Table request failed" }
                : new DistanceMatrixResult
                {
                    Success = true,
                    Distances = osrmResponse.Distances,
                    Durations = osrmResponse.Durations
                };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting distance matrix");
            return new DistanceMatrixResult { Success = false, ErrorMessage = "Internal error" };
        }
    }

    private static List<int> SolveTSP(double[][] distances, bool returnToStart)
    {
        var n = distances.Length;
        var visited = new bool[n];
        var tour = new List<int> { 0 }; // Start from point 0
        visited[0] = true;

        // Greedy nearest neighbor
        for (int step = 1; step < n; step++)
        {
            var current = tour.Last();
            var nearest = -1;
            var nearestDistance = double.MaxValue;

            for (int i = 0; i < n; i++)
            {
                if (!visited[i] && distances[current][i] < nearestDistance)
                {
                    nearest = i;
                    nearestDistance = distances[current][i];
                }
            }

            if (nearest != -1)
            {
                tour.Add(nearest);
                visited[nearest] = true;
            }
        }

        if (returnToStart)
        {
            tour.Add(0);
        }

        return tour;
    }

    private string GetOsrmUrl(TransportMode mode)
    {
        var baseUrl = configuration["OSRM:BaseUrl"];
        var port = mode switch
        {
            TransportMode.Driving => configuration["OSRM:CarPort"],
            TransportMode.Cycling => configuration["OSRM:BicyclePort"],
            TransportMode.Walking => configuration["OSRM:FootPort"],
            _ => configuration["OSRM:CarPort"]
        };

        return $"{baseUrl}:{port}";
    }

    private static string GetProfile(TransportMode mode) => mode switch
    {
        TransportMode.Driving => "driving",
        TransportMode.Cycling => "cycling",
        TransportMode.Walking => "walking",
        _ => "driving"
    };
}

// OSRM Response models
internal class OsrmRouteResponse
{
    public string Code { get; set; } = string.Empty;
    public string? Message { get; set; }
    public List<OsrmRoute> Routes { get; set; } = new();
}

internal class OsrmRoute
{
    public double Distance { get; set; }
    public double Duration { get; set; }
    public OsrmGeometry Geometry { get; set; } = new();
    public List<OsrmLeg> Legs { get; set; } = new();
}

internal class OsrmGeometry
{
    public List<List<double>> Coordinates { get; set; } = new();
}

internal class OsrmLeg
{
    public double Distance { get; set; }
    public double Duration { get; set; }
    public List<OsrmStep> Steps { get; set; } = new();
}

internal class OsrmStep
{
    public double Distance { get; set; }
    public double Duration { get; set; }
    public OsrmManeuver Maneuver { get; set; } = new();
}

internal class OsrmManeuver
{
    public string? Type { get; set; }
    public string? Instruction { get; set; }
}

internal class OsrmTableResponse
{
    public string Code { get; set; } = string.Empty;
    public string? Message { get; set; }
    public double[][] Distances { get; set; } = Array.Empty<double[]>();
    public double[][] Durations { get; set; } = Array.Empty<double[]>();
}

internal class DistanceMatrixResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public double[][]? Distances { get; set; }
    public double[][]? Durations { get; set; }
}