using System.Net.Http;
using System.Net.Http.Json;
using Domain;

namespace DesktopApp;

/// <summary>Typed, async access to the IngestionApi measurement endpoints.</summary>
public interface IMeasurementApiClient
{
    Task<IReadOnlyList<Measurement>> GetMeasurementsAsync(string? type, CancellationToken cancellationToken = default);
}

/// <summary>
/// Typed <see cref="HttpClient"/> client. The base address and lifetime are configured by
/// <c>IHttpClientFactory</c> in <see cref="App"/>; this class only shapes requests/responses.
/// </summary>
public sealed class MeasurementApiClient : IMeasurementApiClient
{
    private readonly HttpClient _http;

    public MeasurementApiClient(HttpClient http) => _http = http;

    public async Task<IReadOnlyList<Measurement>> GetMeasurementsAsync(
        string? type, CancellationToken cancellationToken = default)
    {
        var path = "api/v1/measurements";
        if (!string.IsNullOrWhiteSpace(type))
            path += $"?type={Uri.EscapeDataString(type)}";

        var measurements = await _http.GetFromJsonAsync<List<Measurement>>(path, cancellationToken);
        return measurements ?? [];
    }
}
