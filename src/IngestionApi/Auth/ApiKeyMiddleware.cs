using System.Security.Cryptography;
using System.Text;

namespace IngestionApi.Auth;

/// <summary>
/// Validates the <c>x-api-key</c> header for endpoints marked with
/// <see cref="RequireApiKeyMetadata"/>. Runs after routing but before the endpoint
/// executes (and before request-body binding), so unauthenticated requests are
/// rejected without parsing the body. Unmarked endpoints (health check, Swagger)
/// pass through untouched.
/// </summary>
public sealed class ApiKeyMiddleware
{
    internal const string HeaderName = "x-api-key";

    internal const string ApiKeyConfigName = "Api:Key";

    private readonly RequestDelegate _next;
    private readonly byte[] _expectedKeyBytes;

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;

        var key = configuration[ApiKeyConfigName];
        if (string.IsNullOrWhiteSpace(key))
            throw new InvalidOperationException(
                $"API key is not configured. Set '{ApiKeyConfigName}' in configuration.");

        _expectedKeyBytes = Encoding.UTF8.GetBytes(key);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requiresApiKey =
            context.GetEndpoint()?.Metadata.GetMetadata<RequireApiKeyMetadata>() is not null;

        if (requiresApiKey && !IsValidApiKey(context.Request.Headers))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        await _next(context);
    }

    private bool IsValidApiKey(IHeaderDictionary headers)
    {
        if (!headers.TryGetValue(HeaderName, out var provided))
            return false;

        var providedBytes = Encoding.UTF8.GetBytes(provided.ToString());
        return CryptographicOperations.FixedTimeEquals(providedBytes, _expectedKeyBytes);
    }
}
