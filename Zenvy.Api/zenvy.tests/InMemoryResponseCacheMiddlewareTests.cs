using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Xunit;
using zenvy.api.Infrastructure;

namespace zenvy.tests;

public sealed class InMemoryResponseCacheMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_caches_a_successful_json_get_per_user_and_url()
    {
        var calls = 0;
        RequestDelegate next = async context =>
        {
            calls++;
            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"value\":1}");
        };
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var middleware = new InMemoryResponseCacheMiddleware(next, cache);

        var first = AuthenticatedGet("/api/v1/brands", "17");
        await middleware.InvokeAsync(first);
        var second = AuthenticatedGet("/api/v1/brands", "17");
        await middleware.InvokeAsync(second);

        Assert.Equal(1, calls);
        Assert.Equal("{\"value\":1}", await ReadBody(second));
        Assert.Equal(StatusCodes.Status200OK, second.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_does_not_cache_requests_with_no_cache_or_anonymous_users()
    {
        var calls = 0;
        RequestDelegate next = context =>
        {
            calls++;
            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync("{}");
        };
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var middleware = new InMemoryResponseCacheMiddleware(next, cache);
        var noCache = AuthenticatedGet("/api/v1/brands", "17");
        noCache.Request.Headers.CacheControl = "no-cache";
        var anonymous = new DefaultHttpContext();
        anonymous.Request.Method = HttpMethods.Get;
        anonymous.Request.Path = "/api/v1/brands";
        anonymous.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(noCache);
        await middleware.InvokeAsync(noCache);
        await middleware.InvokeAsync(anonymous);
        await middleware.InvokeAsync(anonymous);

        Assert.Equal(4, calls);
    }

    [Fact]
    public async Task InvokeAsync_does_not_cache_non_json_or_non_success_responses()
    {
        var calls = 0;
        RequestDelegate next = context =>
        {
            calls++;
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            context.Response.ContentType = "text/plain";
            return context.Response.WriteAsync("missing");
        };
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var middleware = new InMemoryResponseCacheMiddleware(next, cache);

        await middleware.InvokeAsync(AuthenticatedGet("/api/v1/brands/404", "17"));
        await middleware.InvokeAsync(AuthenticatedGet("/api/v1/brands/404", "17"));

        Assert.Equal(2, calls);
    }

    private static DefaultHttpContext AuthenticatedGet(string path, string userId)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Get;
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, "test"));
        return context;
    }

    private static async Task<string> ReadBody(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body, leaveOpen: true);
        return await reader.ReadToEndAsync();
    }
}
