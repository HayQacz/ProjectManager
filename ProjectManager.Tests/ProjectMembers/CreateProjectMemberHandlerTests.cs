using Xunit;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Persistence;
using ProjectManager.Features.ProjectMembers.Commands;
using ProjectManager.Entities.Enums;
using ProjectManager.Tests.Dependencies;

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
        // Arrange
        var handler = new CreateProjectMemberHandler(_db);
        var role = ProjectMemberRole.Contributor;
        var command = new CreateProjectMemberCommand(role);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(role, result.Role);
    }
}
