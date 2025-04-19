using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ProjectManager.Entities;
using ProjectManager.Persistence;
using ProjectManager.Features.Users.Commands;
using Xunit;
using Xunit.Abstractions;

namespace ProjectManager.Tests.Integration;

public class AuthorizedEndpointsTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;

    public AuthorizedEndpointsTests(CustomWebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    [Fact]
    public async Task GetProjects_ShouldReturn401_WithoutToken()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/projects");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetProjects_ShouldReturn200_WithValidToken()
    {
        // Arrange
        var client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var user = new User
        {
            Email = "token@user.com",
            FullName = "Token User"
        };
        user.PasswordHash = hasher.HashPassword(user, "TokenPass123");

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var loginHandler = new LoginUserHandler(db, hasher, config);
        var token = await loginHandler.Handle(new LoginUserCommand(user.Email, "TokenPass123"), default);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/projects");

        // Assert
        var body = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"Response Body: {body}");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }
}
