using Xunit;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Entities;
using ProjectManager.Persistence;
using ProjectManager.Features.ProjectMembers.Commands;

namespace ProjectManager.Tests.ProjectMembers;

public class AddProjectMemberToProjectHandlerTests
{
    private readonly AppDbContext _db;

    public AddProjectMemberToProjectHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
    }

    [Fact]
    public async Task Handle_ShouldAddMemberToProject()
    {
        // Arrange
        var project = new Project("Test", "test");
        var member = new ProjectMember { DisplayName = "Tester" };

        _db.Projects.Add(project);
        _db.ProjectMembers.Add(member);
        await _db.SaveChangesAsync();

        var handler = new AddProjectMemberToProjectHandler(_db);
        var command = new AddProjectMemberToProjectCommand(project.Id, member.Id);
        
        // Act
        var result = await handler.Handle(command, default);

        var updated = await _db.Projects
            .Include(p => p.Members)
            .FirstAsync(p => p.Id == project.Id);
        // Assert
        Assert.True(result);
        Assert.Contains(updated.Members, m => m.Id == member.Id);
    }
}
