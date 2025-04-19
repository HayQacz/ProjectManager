using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Persistence;
using ProjectManager.Entities;
using ProjectManager.Entities.Enums;
using ProjectManager.Features.ProjectMembers.Commands;
using ProjectManager.Services.Interfaces;

namespace ProjectManager.Tests.ProjectMembers;

public class ChangeProjectMemberRoleHandlerTests
{
    private readonly AppDbContext _db;
    private readonly Mock<IProjectAuthorizationService> _authMock;

    public ChangeProjectMemberRoleHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        _authMock = new Mock<IProjectAuthorizationService>();
    }

    [Fact]
    public async Task Handle_ShouldChangeMemberRole_WhenAuthorized()
    {
        var project = new Project("Test", "test");
        var member = new ProjectMember { Role = ProjectMemberRole.Viewer, DisplayName = "Tester" };
        var requester = new User();

        project.Members.Add(member);
        _db.Projects.Add(project);
        _db.ProjectMembers.Add(member);
        _db.Users.Add(requester);
        await _db.SaveChangesAsync();

        _authMock.Setup(a => a.CanManageMembers(requester.Id, project.Id)).ReturnsAsync(true);

        var handler = new ChangeProjectMemberRoleHandler(_db, _authMock.Object);
        var command = new ChangeProjectMemberRoleCommand(member.Id, ProjectMemberRole.Manager, requester.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(ProjectMemberRole.Manager, result!.Role);
        Assert.Equal(ProjectMemberRole.Manager, (await _db.ProjectMembers.FindAsync(member.Id))!.Role);
    }
}
