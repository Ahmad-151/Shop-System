using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ShopAPI.DTOClasses;
using ShopAPI.Services;
using System.Net;
using System.Net.Http.Json;

namespace ShopAPI.IntegrationTests;

public sealed class ApiSmokeTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public ApiSmokeTests(ApiFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task LiveHealthEndpoint_ShouldReturnOk()
    {
        using var response = await _client.GetAsync("/health/live");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CategoriesEndpoint_ShouldReturnApiEnvelope()
    {
        using var response = await _client.GetAsync("/api/Category");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<List<CategoryDTO>>>();
        Assert.NotNull(body);
        Assert.True(body!.IsSuccess);
        Assert.Single(body.Data!);
        Assert.Equal("Books", body.Data![0].CategoryName);
    }

    private sealed class ApiEnvelope<T>
    {
        public T? Data { get; set; }
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
    }
}

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:ShopDbConnection", "Server=(localdb);Database=ShopDB;Trusted_Connection=True;TrustServerCertificate=True;");
        builder.UseSetting("JwtSettings:Key", "a-strong-integration-test-key-with-at-least-32-bytes");
        builder.ConfigureTestServices(services =>
        {
            var categories = new Mock<ICategoryService>();
            categories.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<CategoryDTO>
            {
                new() { CategoryId = 1, CategoryName = "Books" }
            });
            var existing = services.Single(x => x.ServiceType == typeof(ICategoryService));
            services.Remove(existing);
            services.AddSingleton(categories.Object);
        });
    }
}
