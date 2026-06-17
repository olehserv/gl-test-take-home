using Domain;
using IngestionApi.Auth;

namespace IngestionApi.Endpoints;

/// <summary>
/// Maps the measurement endpoints. Keeps <c>Program.cs</c> as a thin composition root.
/// </summary>
public static class MeasurementEndpoints
{
    public static IEndpointRouteBuilder MapMeasurementEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/measurements", async (Measurement m, IMeasurementStore store) =>
        {
            var errors = MeasurementValidator.Validate(m);
            if (errors.Count > 0)
                return Results.ValidationProblem(
                    errors.GroupBy(e => e.Field)
                          .ToDictionary(g => g.Key, g => g.Select(e => e.Message).ToArray()));

            await store.AddAsync(m);

            return Results.Accepted($"/api/v1/measurements/{m.MeasurementId}", m);
        })
        .WithMetadata(new RequireApiKeyMetadata());

        app.MapGet("/api/v1/measurements", async (string? type, DateTimeOffset? since, IMeasurementStore store) =>
        {
            var results = await store.QueryAsync(type, since ?? DateTimeOffset.UtcNow.AddMinutes(-5));
            return Results.Ok(results);
        });

        return app;
    }
}
