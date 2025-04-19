using Xunit;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Persistence;
using ProjectManager.Entities;
using ProjectManager.Entities.Enums;
using ProjectManager.Features.ProjectMembers.Commands;

namespace ProjectManager.Tests.ProjectMembers;

public class ChangeProjectMemberRoleHandlerTests
{
    private readonly AppDbContext _db;

    public ChangeProjectMemberRoleHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
    }

    [Fact]
    public async Task Handle_ShouldChangeMemberRole()
    {
        // Arrange
        var member = new ProjectMember
        {
            Role = ProjectMemberRole.Viewer,
            DisplayName = "Jan Tester"
        };

        _db.ProjectMembers.Add(member);
        await _db.SaveChangesAsync();

        var handler = new ChangeProjectMemberRoleHandler(_db);
        var command = new ChangeProjectMemberRoleCommand(member.Id, ProjectMemberRole.Manager);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ProjectMemberRole.Manager, result!.Role);

        var updated = await _db.ProjectMembers.FindAsync(member.Id);
        Assert.Equal(ProjectMemberRole.Manager, updated!.Role);
    }
}
