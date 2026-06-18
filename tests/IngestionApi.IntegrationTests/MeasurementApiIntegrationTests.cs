using System.Net;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Domain;
using IngestionApi.Auth;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace IngestionApi.IntegrationTests;

public class MeasurementApiIntegrationTests : IDisposable
{
    private const string ApiKey = "test-key";

    private const string ApiV1MeasurementPath = "/api/v1/measurements";

    private readonly WebApplicationFactory<Program> _factory;

    public MeasurementApiIntegrationTests()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [ApiKeyMiddleware.ApiKeyConfigName] = ApiKey,
                });
            });
        });
    }

    public void Dispose() => _factory.Dispose();

    private HttpClient CreateClient(bool withApiKey)
    {
        var client = _factory.CreateClient();

        if (withApiKey)
            client.DefaultRequestHeaders.Add(ApiKeyMiddleware.HeaderName, ApiKey);

        return client;
    }

    private static Measurement ValidMeasurement() => new(
        MeasurementId: Guid.NewGuid(),
        Timestamp: DateTimeOffset.UtcNow,
        DeviceId: "device-1",
        PatientId: "patient-1",
        Type: "HeartRate",
        Value: 72,
        Unit: "bpm");

    [Fact]
    public async Task Healthz_NoKey_Returns200()
    {
        var response = await CreateClient(withApiKey: false).GetAsync("/healthz");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetMeasurements_NoKey_Returns200()
    {
        // The GET endpoint is intentionally open (no RequireApiKeyMetadata).
        var response = await CreateClient(withApiKey: false).GetAsync(ApiV1MeasurementPath);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostMeasurement_ValidWithKey_Returns202WithLocation()
    {
        var measurement = ValidMeasurement();

        var response = await CreateClient(withApiKey: true)
            .PostAsJsonAsync(ApiV1MeasurementPath, measurement);

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain(measurement.MeasurementId.ToString());
    }

    [Fact]
    public async Task PostMeasurement_NoKey_Returns401()
    {
        var response = await CreateClient(withApiKey: false)
            .PostAsJsonAsync(ApiV1MeasurementPath, ValidMeasurement());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostMeasurement_MalformedJsonNoKey_Returns401_BeforeBodyBinding()
    {
        var content = new StringContent("{ not valid json", Encoding.UTF8, MediaTypeNames.Application.Json);

        var response = await CreateClient(withApiKey: false)
            .PostAsync(ApiV1MeasurementPath, content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostMeasurement_InvalidWithKey_Returns400WithPerFieldErrors()
    {
        var invalid = new Measurement(
            MeasurementId: Guid.Empty,
            Timestamp: default,
            DeviceId: "",
            PatientId: "patient-1",
            Type: "",
            Value: 0,
            Unit: "bpm");

        var response = await CreateClient(withApiKey: true)
            .PostAsJsonAsync(ApiV1MeasurementPath, invalid);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var errorFields = doc.RootElement.GetProperty("errors")
            .EnumerateObject()
            .Select(p => p.Name.ToLowerInvariant())
            .ToList();

        // Each bad required field carries its own entry (case-insensitive to avoid coupling
        // to the JSON key casing policy).
        errorFields.Should().Contain(["measurementid", "timestamp", "deviceid", "type"]);
    }

    [Fact]
    public async Task PostThenGet_RoundTrip_ReturnsPostedMeasurement()
    {
        var client = CreateClient(withApiKey: true);
        var measurement = ValidMeasurement();

        var post = await client.PostAsJsonAsync(ApiV1MeasurementPath, measurement);
        post.StatusCode.Should().Be(HttpStatusCode.Accepted);

        var returned = await client.GetFromJsonAsync<List<Measurement>>(
            $"{ApiV1MeasurementPath}?type={measurement.Type}");

        returned.Should().NotBeNull();
        returned!.Should().ContainSingle()
            .Which.MeasurementId.Should().Be(measurement.MeasurementId);
    }
}
