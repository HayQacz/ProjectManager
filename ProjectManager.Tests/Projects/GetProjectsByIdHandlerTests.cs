using Xunit;
using Microsoft.EntityFrameworkCore;
using Moq;
using ProjectManager.Persistence;
using ProjectManager.Entities;
using ProjectManager.Services.Interfaces;
using ProjectManager.Features.Projects.Queries;

namespace ProjectManager.Tests.Projects;

public class GetProjectByIdHandlerTests
{
    private readonly AppDbContext _db;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly Mock<IProjectAuthorizationService> _authServiceMock;

    public GetProjectByIdHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        _userContextMock = new Mock<IUserContext>();
        _authServiceMock = new Mock<IProjectAuthorizationService>();
    }

    [Fact]
    public async Task Handle_ShouldReturnProject_WhenUserCanViewProject()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var project = new Project("Test", "Description");
        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

        _userContextMock.Setup(u => u.UserId).Returns(userId);
        _authServiceMock.Setup(auth => auth.CanViewProject(userId, project.Id)).ReturnsAsync(true);

        var handler = new GetProjectByIdHandler(_db, _userContextMock.Object, _authServiceMock.Object);
        var query = new GetProjectByIdQuery(project.Id);

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(project.Id, result.Id);
        Assert.Equal("Test", result.Name);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenUserCannotViewProject()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var project = new Project("Hidden", "Secret");
        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

        _userContextMock.Setup(u => u.UserId).Returns(userId);
        _authServiceMock.Setup(auth => auth.CanViewProject(userId, project.Id)).ReturnsAsync(false);

        var handler = new GetProjectByIdHandler(_db, _userContextMock.Object, _authServiceMock.Object);
        var query = new GetProjectByIdQuery(project.Id);

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        Assert.Null(result);
    }
}
