using System.Net.Http.Headers;

namespace TreasureHuntApp.Client.Services.Http;

public static class HttpClientConfiguration
{
    public static void ConfigureHttpClient(HttpClient httpClient, string baseAddress)
    {
        httpClient.BaseAddress = new Uri(baseAddress);
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        // Timeout beállítás
        httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public static HttpClientHandler CreateHttpClientHandler() => new HttpClientHandler()
    {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
    };
}
