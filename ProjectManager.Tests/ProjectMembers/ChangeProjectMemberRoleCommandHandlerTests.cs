using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjectManager.Persistence;
using ProjectManager.Entities;
using ProjectManager.Entities.Enums;
using ProjectManager.Features.ProjectMembers.Commands;
using ProjectManager.Features.Projects.Commands;
using ProjectManager.Services.Interfaces;
using ProjectManager.Tests.Dependencies;
using System.Threading;

namespace ProjectManager.Tests.ProjectMembers;

public class ChangeProjectMemberRoleCommandHandlerTests
        : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly Mock<IProjectAuthorizationService>   _authMock = new();

    public ChangeProjectMemberRoleCommandHandlerTests(
        CustomWebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task Handle_ShouldChangeRole_WhenAuthorized()
    {
        //Arrange
        Guid projectId, memberUserId, requesterId;

        using var scope = _factory.Services.CreateScope();
        var sp  = scope.ServiceProvider;
        var db  = sp.GetRequiredService<AppDbContext>();

        var requester = new User { Email = "owner@x.pl" };
        requesterId   = requester.Id;
        db.Users.Add(requester);

        var fakeCtx = (FakeUserContext)sp.GetRequiredService<IUserContext>();
        fakeCtx.UserId = requesterId;

        var create = new CreateProjectCommandHandler(db, fakeCtx);
        projectId  = await create.Handle(
            new CreateProjectCommand("Proj", "Desc"), CancellationToken.None);

        var memberUser = new User { Email = "member@x.pl" };
        memberUserId   = memberUser.Id;

        var member = new ProjectMember
        {
            ProjectId = projectId,
            UserId    = memberUserId,
            User      = memberUser,
            Role      = ProjectMemberRole.Viewer
        };

        db.Users.Add(memberUser);
        db.ProjectMembers.Add(member);
        await db.SaveChangesAsync();

        _authMock.Setup(a => a.CanManageMembers(requesterId, projectId))
                 .ReturnsAsync(true);

        var handler = new ChangeProjectMemberRoleCommandHandler(db, _authMock.Object);
        var cmd     = new ChangeProjectMemberRoleCommand(
                          memberUserId,
                          ProjectMemberRole.Manager,
                          requesterId,
                          projectId);

        //Act
        var dto = await handler.Handle(cmd, CancellationToken.None);

        //Assert
        Assert.NotNull(dto);
        Assert.Equal(ProjectMemberRole.Manager, dto!.Role);

        var stored = await db.ProjectMembers
                             .FirstAsync(pm => pm.UserId == memberUserId &&
                                               pm.ProjectId == projectId);
        Assert.Equal(ProjectMemberRole.Manager, stored.Role);
    }

    [Fact]
    public async Task Handle_ShouldThrow_Unauthorized()
    {
        // Arrange
        Guid projectId, memberUserId, requesterId;

        using var scope = _factory.Services.CreateScope();
        var sp = scope.ServiceProvider;
        var db = sp.GetRequiredService<AppDbContext>();

        var owner = new User { Email = "owner@x.pl" };
        var ctx   = (FakeUserContext)sp.GetRequiredService<IUserContext>();
        ctx.UserId = owner.Id;

        projectId = await new CreateProjectCommandHandler(db, ctx)
                     .Handle(new CreateProjectCommand("P2", "D2"), CancellationToken.None);

        var memberUser = new User { Email = "member@x.pl" };
        memberUserId   = memberUser.Id;

        db.Users.Add(memberUser);
        db.ProjectMembers.Add(new ProjectMember
        {
            ProjectId = projectId,
            UserId    = memberUserId,
            User      = memberUser,
            Role      = ProjectMemberRole.Viewer
        });

        var requester = new User();
        requesterId   = requester.Id;
        db.Users.Add(requester);
        await db.SaveChangesAsync();

        _authMock.Setup(a => a.CanManageMembers(requesterId, projectId))
                 .ReturnsAsync(false);

        var handler = new ChangeProjectMemberRoleCommandHandler(db, _authMock.Object);
        var cmd = new ChangeProjectMemberRoleCommand(
                      memberUserId,
                      ProjectMemberRole.Manager,
                      requesterId,
                      projectId);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(cmd, CancellationToken.None));
    }
}
