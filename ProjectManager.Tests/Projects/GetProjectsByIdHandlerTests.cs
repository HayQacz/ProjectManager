using Xunit;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Persistence;
using ProjectManager.Features.Projects.Queries;
using ProjectManager.Entities;

namespace ProjectManager.Tests.Projects;

public class GetProjectByIdHandlerTests
{
    private readonly AppDbContext _db;

    public GetProjectByIdHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
    }

    [Fact]
    public async Task Handle_ShouldReturnProject_WhenExists()
    {
        var project = new Project("From Test", "Desc");
        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

        var handler = new GetProjectByIdHandler(_db);
        var result = await handler.Handle(new GetProjectByIdQuery(project.Id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(project.Id, result!.Id);
    }
}
