using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Domain;
using IngestionApi.Auth;
using IngestionApi.Endpoints;
using IngestionApi.Swagger;

namespace IngestionApi;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();
        builder.Services.AddSingleton<IMeasurementStore, InMemoryMeasurementStore>();
        builder.Services.AddProblemDetails();

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
            foreach (var description in provider.ApiVersionDescriptions)
                options.SwaggerEndpoint(
                    $"/swagger/{description.GroupName}/swagger.json",
                    description.GroupName.ToUpperInvariant());
        });

        app.UseMiddleware<ApiKeyMiddleware>();

        app.MapGet("/healthz", () => Results.Ok(new { status = "healthy" }));
        app.MapMeasurementEndpoints();

        await app.RunAsync();
    }
}
