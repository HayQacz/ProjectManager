using Xunit;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Persistence;
using ProjectManager.Entities;
using ProjectManager.Features.Users.Commands;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Text;
using Xunit.Abstractions;

namespace ProjectManager.Tests.Users;

public class RegisterAndLoginUserTests
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<User> _hasher;
    private readonly IConfiguration _config;
    private readonly ITestOutputHelper _output;

    public RegisterAndLoginUserTests(ITestOutputHelper output)
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
    public async Task User_ShouldBeAbleTo_Register_And_Login_WithSameCredentials()
    {

        // Arrange
        var registerHandler = new RegisterUserHandler(_db, _hasher);
        var loginHandler = new LoginUserHandler(_db, _hasher, _config);

        var email = "test@user.com";
        var password = "P@ssword123";
        var fullName = "Test User";

        var registerCommand = new RegisterUserCommand(email, fullName, password);

        // Act 
        var registeredUser = await registerHandler.Handle(registerCommand, CancellationToken.None);

        // Act 
        var loginCommand = new LoginUserCommand(email, password);
        var token = await loginHandler.Handle(loginCommand, CancellationToken.None);

        // Assert
        Assert.NotNull(registeredUser);
        Assert.NotEqual(Guid.Empty, registeredUser.Id);
        Assert.False(string.IsNullOrEmpty(token));
        Assert.StartsWith("eyJ", token);
    }
}
