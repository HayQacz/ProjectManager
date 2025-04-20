using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Entities;
using ProjectManager.Persistence;
using ProjectManager.Features.ProjectMembers.Commands;
using ProjectManager.Services.Interfaces;
using System.Threading;

namespace ProjectManager.Tests.ProjectMembers;

public class RemoveProjectMemberFromProjectCommandHandlerTests
{
    private readonly AppDbContext _db;
    private readonly Mock<IProjectAuthorizationService> _authMock = new();

    public RemoveProjectMemberFromProjectCommandHandlerTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(opts);
    }

    [Fact]
    public async Task Handle_ShouldRemoveMember_WhenAuthorized()
    {
        // Arrange
        var project = new Project("Test", "Desc");
        var member  = new ProjectMember { UserId = Guid.NewGuid(), DisplayName = "X" };
        project.Members.Add(member);

        var requester = new User { Id = Guid.NewGuid() };

        _db.Projects.Add(project);
        _db.Users.Add(requester);
        await _db.SaveChangesAsync();

        _authMock.Setup(a => a.CanManageMembers(requester.Id, project.Id))
            .ReturnsAsync(true);

        var handler = new RemoveProjectMemberFromProjectCommandHandler(_db, _authMock.Object);
        var cmd = new RemoveProjectMemberFromProjectCommand(project.Id, member.UserId, requester.Id);

        // Act
        var ok = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        var reloaded = await _db.Projects.Include(p => p.Members)
            .FirstAsync(p => p.Id == project.Id);

        Assert.True(ok);
        Assert.DoesNotContain(reloaded.Members, m => m.UserId == member.UserId);
    }
}
