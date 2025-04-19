using Xunit;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Features.Users.Commands;
using ProjectManager.Persistence;
using ProjectManager.Entities;
using ProjectManager.Features.Users.Models;
using Microsoft.AspNetCore.Identity;

namespace ProjectManager.Tests.Users;

public class RegisterUserHandlerTests
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<User> _hasher;

    public RegisterUserHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) 
            .Options;

        _db = new AppDbContext(options);
        _hasher = new PasswordHasher<User>();
    }

    [Fact]
    public async Task Handle_ShouldRegisterUser_WhenDataIsValid()
    {
        // Arrange
        var handler = new RegisterUserHandler(_db, _hasher);
        var command = new RegisterUserCommand(
            Email: "test@user.com",
            FullName: "Test User",
            Password: "12345SuperSecure!"
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var user = await _db.Users.FindAsync(result.Id);

        Assert.NotNull(user);
        Assert.Equal(command.Email, user!.Email);
        Assert.Equal(command.FullName, user.FullName);
        Assert.NotEmpty(user.PasswordHash);
        Assert.True(_hasher.VerifyHashedPassword(user, user.PasswordHash, command.Password) != PasswordVerificationResult.Failed);
    }
}
