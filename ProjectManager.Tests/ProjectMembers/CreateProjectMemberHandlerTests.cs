using Xunit;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Persistence;
using ProjectManager.Features.ProjectMembers.Commands;

namespace ProjectManager.Tests.ProjectMembers;

public class CreateProjectMemberHandlerTests
{
    private readonly AppDbContext _db;

    public CreateProjectMemberHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
    }

    [Fact]
    public async Task Handle_ShouldCreateMember()
    {
        var handler = new CreateProjectMemberHandler(_db);
        var command = new CreateProjectMemberCommand("Dev");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Dev", result.Role);
    }
}
