using Xunit;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Features.Projects.Commands;
using ProjectManager.Persistence;
using ProjectManager.Entities.Enums;

namespace ProjectManager.Tests.Projects;

public class CreateProjectHandlerTests
{
    private readonly AppDbContext _db;

    public CreateProjectHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) 
            .Options;

        _db = new AppDbContext(options);
    }

    [Fact]
    public async Task Handle_ShouldCreateProjectWithDetails_WhenDetailsProvided()
    {
        // Arrange
        var handler = new CreateProjectHandler(_db);
        var command = new CreateProjectCommand(
            Name: "Test App",
            Description: "Testing project details",
            Deadline: new DateTime(2025, 12, 31),
            TechnologiesUsed: "ASP.NET Core, PostgreSQL",
            Status: ProjectStatus.InProgress,
            Visibility: ProjectVisibility.Internal,
            IsCommercial: true
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var project = await _db.Projects
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == result);

        Assert.NotNull(project);
        Assert.Equal("Test App", project!.Name);
        Assert.Equal("Testing project details", project.Description);

        Assert.NotNull(project.Details);
        Assert.Equal(new DateTime(2025, 12, 31), project.Details!.Deadline);
        Assert.Equal("ASP.NET Core, PostgreSQL", project.Details.TechnologiesUsed);
        Assert.Equal(ProjectStatus.InProgress, project.Details.Status);
        Assert.Equal(ProjectVisibility.Internal, project.Details.Visibility);
        Assert.True(project.Details.IsCommercial);
    }
}
