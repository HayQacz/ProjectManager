using MediatR;
using ProjectManager.Entities;
using ProjectManager.Entities.Enums;
using ProjectManager.Persistence;
using ProjectManager.Services.Interfaces;

namespace ProjectManager.Features.Projects.Commands;

public record CreateProjectCommand(
    string Name,
    string Description,
    DateTime? Deadline         = null,
    string? TechnologiesUsed   = null,
    ProjectStatus? Status      = null,
    ProjectVisibility? Visibility = null,
    bool? IsCommercial         = null
) : IRequest<Guid>;

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Guid>
{
    private readonly AppDbContext _db;
    private readonly IUserContext _user;

    public CreateProjectCommandHandler(AppDbContext db, IUserContext userContext)
    {
        _db   = db;
        _user = userContext;
    }

    public async Task<Guid> Handle(CreateProjectCommand req, CancellationToken ct)
    {
        var project = new Project(req.Name, req.Description);

        if (req.Deadline.HasValue || req.TechnologiesUsed is not null)
        {
            project.Details = new ProjectDetails
            {
                Deadline         = req.Deadline        ?? DateTime.UtcNow.AddMonths(1),
                Status           = req.Status          ?? ProjectStatus.Created,
                TechnologiesUsed = req.TechnologiesUsed?? string.Empty,
                Visibility       = req.Visibility      ?? ProjectVisibility.Private,
                IsCommercial     = req.IsCommercial    ?? false
            };
        }

        var owner = new ProjectMember
        {
            Project   = project,
            UserId    = _user.UserId,
            Role      = ProjectMemberRole.Owner
        };
        project.Members.Add(owner);

        _db.Projects.Add(project);
        await _db.SaveChangesAsync(ct);

        return project.Id;
    }
}
