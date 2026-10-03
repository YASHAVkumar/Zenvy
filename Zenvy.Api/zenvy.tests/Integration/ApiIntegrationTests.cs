using System.Net;
using System.Net.Http.Json;
using Moq;
using Xunit;
using zenvy.Application.DTOs.Category;

namespace zenvy.tests.Integration;

public sealed class ApiIntegrationTests : IClassFixture<TestApiFactory>
{
    private readonly TestApiFactory _factory;
    private readonly HttpClient _client;

    public ApiIntegrationTests(TestApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Root_endpoint_is_available_through_the_real_application_pipeline()
    {
        var response = await _client.GetAsync("/", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Hello World", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Unknown_route_returns_the_standard_api_error_envelope()
    {
        var response = await _client.GetAsync("/not-a-route", TestContext.Current.CancellationToken);
        var body = await response.Content.ReadFromJsonAsync<ApiEnvelope>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(body);
        Assert.False(body!.Success);
        Assert.Equal("The requested resource was not found.", body.Message);
        Assert.False(string.IsNullOrWhiteSpace(body.TraceId));
    }

    [Fact]
    public async Task Authorized_categories_endpoint_uses_mock_service_and_wraps_data()
    {
        _factory.Categories.Reset();
        _factory.Categories.Setup(x => x.GetAllAsync()).ReturnsAsync(new[]
        {
            new CategoryResponse { CategoryId = 3, CategoryName = "Accessories", IsActive = true }
        });

        var response = await _client.GetAsync("/api/v1/categories", TestContext.Current.CancellationToken);
        var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<List<CategoryResponse>>>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.True(body!.Success);
        Assert.Single(body.Data!);
        Assert.Equal("Accessories", body.Data![0].CategoryName);
        _factory.Categories.Verify(x => x.GetAllAsync(), Times.Once);
    }

    private class ApiEnvelope<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string TraceId { get; set; } = string.Empty;
        public T? Data { get; set; }
    }

    private sealed class ApiEnvelope : ApiEnvelope<object>;
}
