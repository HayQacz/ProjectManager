using Xunit;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Entities;
using ProjectManager.Persistence;
using ProjectManager.Features.ProjectMembers.Commands;

namespace ProjectManager.Tests.ProjectMembers;

public class RemoveProjectMemberFromProjectHandlerTests
{
    private readonly AppDbContext _db;

    public RemoveProjectMemberFromProjectHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
    }

    [Fact]
    public async Task Handle_ShouldRemoveMemberFromProject()
    {
        // Arrange
        var project = new Project("Remove Test", "test");
        var member = new ProjectMember { DisplayName = "Do usunięcia" };

        project.Members.Add(member);
        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

        var handler = new RemoveProjectMemberFromProjectHandler(_db);
        var command = new RemoveProjectMemberFromProjectCommand(project.Id, member.Id);
        
        // Act
        var result = await handler.Handle(command, default);

        var updated = await _db.Projects
            .Include(p => p.Members)
            .FirstAsync(p => p.Id == project.Id);
        
        // Assert
        Assert.True(result);
        Assert.DoesNotContain(updated.Members, m => m.Id == member.Id);
    }
}
