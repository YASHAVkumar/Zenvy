using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;

namespace zenvy.api.Infrastructure;

public sealed class InMemoryResponseCacheMiddleware(
    RequestDelegate next,
    IMemoryCache cache)
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);

    public async Task InvokeAsync(HttpContext context)
    {
        if (!ShouldCache(context))
        {
            await next(context);
            return;
        }

        var cacheKey = BuildCacheKey(context);
        if (cache.TryGetValue(cacheKey, out CachedResponse? cached) && cached is not null)
        {
            context.Response.StatusCode = cached.StatusCode;
            context.Response.ContentType = cached.ContentType;
            await context.Response.Body.WriteAsync(cached.Body);
            return;
        }

        var originalBody = context.Response.Body;
        await using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await next(context);
            if (context.Response.StatusCode == StatusCodes.Status200OK &&
                context.Response.ContentType?.Contains("json", StringComparison.OrdinalIgnoreCase) == true)
            {
                var body = responseBody.ToArray();
                cache.Set(cacheKey, new CachedResponse(context.Response.StatusCode, context.Response.ContentType, body), CacheDuration);
            }

            responseBody.Position = 0;
            await responseBody.CopyToAsync(originalBody);
        }
        finally
        {
            context.Response.Body = originalBody;
        }
    }

    private static bool ShouldCache(HttpContext context) =>
        HttpMethods.IsGet(context.Request.Method) &&
        context.User.Identity?.IsAuthenticated == true &&
        !context.Request.Headers["Cache-Control"].ToString().Contains("no-cache", StringComparison.OrdinalIgnoreCase) &&
        !context.Request.Path.StartsWithSegments("/api/v1/auth") &&
        !context.Request.Path.StartsWithSegments("/hubs") &&
        !context.Request.Path.StartsWithSegments("/swagger");

    private static string BuildCacheKey(HttpContext context)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
        return $"api-response:{userId}:{context.Request.Path}{context.Request.QueryString}";
    }

    private sealed record CachedResponse(int StatusCode, string? ContentType, byte[] Body);
}