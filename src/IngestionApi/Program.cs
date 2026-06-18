using Domain;
using IngestionApi.Auth;
using IngestionApi.Endpoints;

namespace IngestionApi;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddSingleton<IMeasurementStore, InMemoryMeasurementStore>();
        builder.Services.AddProblemDetails();

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseMiddleware<ApiKeyMiddleware>();

        app.MapGet("/healthz", () => Results.Ok(new { status = "healthy" }));
        app.MapMeasurementEndpoints();

        await app.RunAsync();
    }
}
