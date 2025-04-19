using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using ProjectManager.Entities;
using ProjectManager.Persistence;
using ProjectManager.Features.Users.Commands;
using System.Text;
using Xunit.Abstractions;

namespace ProjectManager.Tests.Users;

public class LoginUserInvalidPasswordTests
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<User> _hasher;
    private readonly IConfiguration _config;
    private readonly ITestOutputHelper _output;

    public LoginUserInvalidPasswordTests(ITestOutputHelper output)
    {
        _output = output;

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        _hasher = new PasswordHasher<User>();

        _config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldThrowUnauthorized()
    {
        // Arrange
        var email = "wrong@user.com";
        var correctPassword = "Correct123";
        var wrongPassword = "WrongPass123";

        var registerHandler = new RegisterUserHandler(_db, _hasher);
        var loginHandler = new LoginUserHandler(_db, _hasher, _config);

        var registerCommand = new RegisterUserCommand(email, "Wrong Name", correctPassword);
        await registerHandler.Handle(registerCommand, CancellationToken.None);

        var loginCommand = new LoginUserCommand(email, wrongPassword);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            loginHandler.Handle(loginCommand, CancellationToken.None));

        _output.WriteLine($"[DEBUG] Exception message: {ex.Message}");
    }
}
