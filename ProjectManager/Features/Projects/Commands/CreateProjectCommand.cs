using MediatR;
using ProjectManager.Entities;
using ProjectManager.Entities.Enums;
using ProjectManager.Persistence;

namespace ProjectManager.Features.Projects.Commands;

public record CreateProjectCommand(
    string Name,
    string Description,
    DateTime? Deadline = null,
    string? TechnologiesUsed = null,
    ProjectStatus? Status = null,
    ProjectVisibility? Visibility = null,
    bool? IsCommercial = null
) : IRequest<Guid>;

public class CreateProjectHandler : IRequestHandler<CreateProjectCommand, Guid>
{
    private readonly AppDbContext _db;

    public CreateProjectHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Guid> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = new Project(request.Name, request.Description);

        if (request.Deadline.HasValue || request.TechnologiesUsed is not null)
        {
            project.Details = new ProjectDetails
            {
                Deadline = request.Deadline ?? DateTime.UtcNow.AddMonths(1),
                Status = request.Status ?? ProjectStatus.Created,
                TechnologiesUsed = request.TechnologiesUsed ?? string.Empty,
                Visibility = request.Visibility ?? ProjectVisibility.Private,
                IsCommercial = request.IsCommercial ?? false
            };
        }

        _db.Projects.Add(project);
        await _db.SaveChangesAsync(cancellationToken);
        return project.Id;
    }
}
