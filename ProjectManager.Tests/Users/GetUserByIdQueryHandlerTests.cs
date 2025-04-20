using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ProjectManager.Persistence;
using ProjectManager.Entities;
using ProjectManager.Features.Users.Queries;

namespace ProjectManager.Tests.Users;

public class GetUserByIdQueryHandlerTests
{
    private readonly AppDbContext _db;

    public GetUserByIdQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
    }

    [Fact]
    public async Task Handle_ShouldReturnUser_WhenExists()
    {
        var user = new User
        {
            Email = "existing@user.com",
            FullName = "User",
            PasswordHash = "hashed"
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var handler = new GetUserByIdQueryHandler(_db);
        var result = await handler.Handle(new GetUserByIdQuery(user.Id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result!.Id);
        Assert.Equal(user.Email, result.Email);
    }
}
