using Asp.Versioning;
using Asp.Versioning.Builder;
using Domain;
using IngestionApi.Auth;

namespace IngestionApi.Endpoints;

public static class MeasurementEndpoints
{
    private static readonly ApiVersion V1 = new(1, 0);

    public static IEndpointRouteBuilder MapMeasurementEndpoints(this IEndpointRouteBuilder app)
    {
        ApiVersionSet versionSet = app.NewApiVersionSet()
            .HasApiVersion(V1)
            .ReportApiVersions()
            .Build();

        // The version is declared once here; the segment {version:apiVersion} keeps the
        // existing "/api/v1/measurements" URLs working and lets a future v2 be added cleanly.
        var v = app.MapGroup("/api/v{version:apiVersion}")
            .WithApiVersionSet(versionSet);

        v.MapPost("/measurements", async (Measurement m, IMeasurementStore store) =>
        {
            var errors = MeasurementValidator.Validate(m);
            if (errors.Count > 0)
                return Results.ValidationProblem(
                    errors.GroupBy(e => e.Field)
                          .ToDictionary(g => g.Key, g => g.Select(e => e.Message).ToArray()));

            await store.AddAsync(m);

            return Results.Accepted($"/api/v1/measurements/{m.MeasurementId}", m); //todo: fix to use Type instead of MeasurementId
        })
        .MapToApiVersion(V1)
        .WithMetadata(new RequireApiKeyMetadata());

        v.MapGet("/measurements", async (string? type, DateTimeOffset? since, IMeasurementStore store) =>
        {
            var results = await store.QueryAsync(type, since ?? DateTimeOffset.UtcNow.AddMinutes(-5));
            return Results.Ok(results);
        })
        .MapToApiVersion(V1);

        return app;
    }
}
