namespace IngestionApi.Auth;

/// <summary>
/// Marker metadata. Endpoints tagged with this require a valid <c>x-api-key</c>
/// header, enforced by <see cref="ApiKeyMiddleware"/>.
/// </summary>
public sealed class RequireApiKeyMetadata;
