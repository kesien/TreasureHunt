using TreasureHuntApp.Client.Services;
using TreasureHuntApp.Client.Services.Api;
using TreasureHuntApp.Client.Services.Http;
using TreasureHuntApp.Client.Services.Utilities;

namespace TreasureHuntApp.Client.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, string baseApiUrl)
    {
        // HttpClient konfiguráció
        services.AddHttpClient("ApiService", client =>
        {
            HttpClientConfiguration.ConfigureHttpClient(client, baseApiUrl);
        });

        services.AddScoped<IApiService, ApiService>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<ITeamService, TeamService>();
        services.AddScoped<ILocationService, LocationService>();

        services.AddScoped<INotificationService, NotificationService>();

        return services;
    }
}
