using Xunit;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Features.Projects.Commands;
using ProjectManager.Persistence;
using ProjectManager.Entities.Enums;
using ProjectManager.Entities;
using ProjectManager.Services.Interfaces;
using System.Threading;

namespace ProjectManager.Tests.Projects;

public class CreateProjectCommandHandlerTests
{
    private readonly AppDbContext   _db;
    private readonly IUserContext   _userCtx;

    public CreateProjectCommandHandlerTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
                   .UseInMemoryDatabase(Guid.NewGuid().ToString())
                   .Options;

        _db = new AppDbContext(opts);

        _userCtx = new FakeUserContext();   
    }

    [Fact]
    public async Task Handle_ShouldCreateProject_WithDetails_AndOwner()
    {
        // Arrange 
        var handler = new CreateProjectCommandHandler(_db, _userCtx);   

        var cmd = new CreateProjectCommand(
            Name:            "Test App",
            Description:     "Testing project details",
            Deadline:        new DateTime(2025, 12, 31),
            TechnologiesUsed:"ASP.NET Core, PostgreSQL",
            Status:          ProjectStatus.InProgress,
            Visibility:      ProjectVisibility.Internal,
            IsCommercial:    true);

        // Act 
        var id = await handler.Handle(cmd, CancellationToken.None);

        // Assert 
        var project = await _db.Projects
                               .Include(p => p.Details)
                               .Include(p => p.Members)
                               .FirstOrDefaultAsync(p => p.Id == id);

        Assert.NotNull(project);
        Assert.Equal("Test App",       project!.Name);
        Assert.Equal("Testing project details", project.Description);

        Assert.NotNull(project.Details);
        Assert.Equal(new DateTime(2025, 12, 31),  project.Details!.Deadline);
        Assert.Equal("ASP.NET Core, PostgreSQL",  project.Details.TechnologiesUsed);
        Assert.Equal(ProjectStatus.InProgress,    project.Details.Status);
        Assert.Equal(ProjectVisibility.Internal,  project.Details.Visibility);
        Assert.True(project.Details.IsCommercial);

        var owner = Assert.Single(project.Members);
        Assert.Equal(_userCtx.UserId,   owner.UserId);
        Assert.Equal(ProjectMemberRole.Owner, owner.Role);
    }

    private class FakeUserContext : IUserContext
    {
        public Guid UserId { get; } = Guid.NewGuid();
    }
}
