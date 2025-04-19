using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Entities;
using ProjectManager.Persistence;
using ProjectManager.Features.ProjectMembers.Commands;
using ProjectManager.Services.Interfaces;
using System.Threading;

namespace ProjectManager.Tests.ProjectMembers;

public class AddProjectMemberToProjectHandlerTests
{
    private readonly AppDbContext _db;
    private readonly Mock<IProjectAuthorizationService> _authMock;

    public AddProjectMemberToProjectHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        _authMock = new Mock<IProjectAuthorizationService>();
    }

    [Fact]
    public async Task Handle_ShouldAddMemberToProject_WhenAuthorized()
    {
        // Arrange
        var project = new Project("Test", "test");
        var member = new ProjectMember { DisplayName = "Tester" };
        var requester = new User();

        _db.Projects.Add(project);
        _db.ProjectMembers.Add(member);
        _db.Users.Add(requester);
        await _db.SaveChangesAsync();

        _authMock.Setup(a => a.CanManageMembers(requester.Id, project.Id)).ReturnsAsync(true);

        var handler = new AddProjectMemberToProjectHandler(_db, _authMock.Object);
        var command = new AddProjectMemberToProjectCommand(project.Id, member.Id, requester.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await _db.Projects.Include(p => p.Members).FirstAsync(p => p.Id == project.Id);
        Assert.True(result);
        Assert.Contains(updated.Members, m => m.Id == member.Id);
    }
}
