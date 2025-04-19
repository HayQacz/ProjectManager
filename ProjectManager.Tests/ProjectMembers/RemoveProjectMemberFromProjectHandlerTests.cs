using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Entities;
using ProjectManager.Persistence;
using ProjectManager.Features.ProjectMembers.Commands;
using ProjectManager.Services.Interfaces;

namespace ProjectManager.Tests.ProjectMembers;

public class RemoveProjectMemberFromProjectHandlerTests
{
    private readonly AppDbContext _db;
    private readonly Mock<IProjectAuthorizationService> _authMock;

    public RemoveProjectMemberFromProjectHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        _authMock = new Mock<IProjectAuthorizationService>();
    }

    [Fact]
    public async Task Handle_ShouldRemoveMemberFromProject_WhenAuthorized()
    {
        var project = new Project("Test Project", "desc");
        var member = new ProjectMember { DisplayName = "ToRemove" };
        var requester = new User();

        project.Members.Add(member);
        _db.Projects.Add(project);
        _db.ProjectMembers.Add(member);
        _db.Users.Add(requester);
        await _db.SaveChangesAsync();

        _authMock.Setup(a => a.CanManageMembers(requester.Id, project.Id)).ReturnsAsync(true);

        var handler = new RemoveProjectMemberFromProjectHandler(_db, _authMock.Object);
        var command = new RemoveProjectMemberFromProjectCommand(project.Id, member.Id, requester.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        var updated = await _db.Projects.Include(p => p.Members).FirstAsync(p => p.Id == project.Id);
        Assert.True(result);
        Assert.DoesNotContain(updated.Members, m => m.Id == member.Id);
    }
}
