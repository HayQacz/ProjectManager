using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjectManager.Persistence;
using ProjectManager.Entities;
using ProjectManager.Features.ProjectMembers.Commands;
using ProjectManager.Services.Interfaces;
using ProjectManager.Tests.Dependencies;
using System.Threading;

namespace ProjectManager.Tests.ProjectMembers;

public class AddProjectMemberToProjectCommandHandlerTests
        : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly Mock<IProjectAuthorizationService>   _authMock = new();

    public AddProjectMemberToProjectCommandHandlerTests(
        CustomWebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task Handle_ShouldAddMember_WhenAuthorized()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var project = new Project("Test Project", "Desc");
        var user    = new User { Email = "x@y.z", FullName = "X" };

        db.Projects.Add(project);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        _authMock.Setup(a => a.CanManageMembers(user.Id, project.Id))
                 .ReturnsAsync(true);

        var handler = new AddProjectMemberToProjectCommandHandler(db, _authMock.Object);
        var cmd     = new AddProjectMemberToProjectCommand(project.Id, user.Id, user.Id);

        // Act
        var ok = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        Assert.True(ok);

        var updated = await db.Projects.Include(p => p.Members)
                                       .FirstAsync(p => p.Id == project.Id);

        Assert.Contains(updated.Members, m => m.UserId == user.Id);
    }

    [Fact]
    public async Task Handle_ShouldThrow_Unauthorized()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var project = new Project("Test", "Desc");
        var user    = new User { Email = "u@u.pl" };

        db.Projects.Add(project);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        _authMock.Setup(a => a.CanManageMembers(user.Id, project.Id))
                 .ReturnsAsync(false);

        var handler = new AddProjectMemberToProjectCommandHandler(db, _authMock.Object);
        var cmd     = new AddProjectMemberToProjectCommand(project.Id, user.Id, user.Id);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(cmd, CancellationToken.None));
    }
}
