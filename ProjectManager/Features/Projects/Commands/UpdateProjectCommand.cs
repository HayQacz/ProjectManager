using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Entities.Enums;
using ProjectManager.Persistence;
using ProjectManager.Services.Interfaces;

namespace ProjectManager.Features.Projects.Commands;

public record UpdateProjectCommand(
    Guid Id,
    string Name,
    string Description,
    DateTime? Deadline          = null,
    string? TechnologiesUsed    = null,
    ProjectStatus? Status       = null,
    ProjectVisibility? Visibility = null,
    bool? IsCommercial          = null
) : IRequest<bool>;

public class UpdateProjectHandler : IRequestHandler<UpdateProjectCommand, bool>
{
    private readonly AppDbContext _db;
    private readonly IUserContext _userContext;

    public UpdateProjectHandler(AppDbContext db, IUserContext userContext)
    {
        _db          = db;
        _userContext = userContext;
    }

    public async Task<bool> Handle(UpdateProjectCommand request, CancellationToken ct)
    {
        var project = await _db.Projects
                               .Include(p => p.Details)
                               .FirstOrDefaultAsync(p => p.Id == request.Id, ct);
        if (project is null) return false;

        var member = await _db.ProjectMembers
                              .FirstOrDefaultAsync(m =>
                                   m.ProjectId == request.Id &&
                                   m.UserId    == _userContext.UserId, ct);

        if (member is null || member.Role is not (ProjectMemberRole.Owner or ProjectMemberRole.Manager))
            return false;

        project.Update(request.Name, request.Description);

        if (project.Details is not null)
        {
            project.Details.Deadline         = request.Deadline        ?? project.Details.Deadline;
            project.Details.Status           = request.Status          ?? project.Details.Status;
            project.Details.TechnologiesUsed = request.TechnologiesUsed?? project.Details.TechnologiesUsed;
            project.Details.Visibility       = request.Visibility      ?? project.Details.Visibility;
            project.Details.IsCommercial     = request.IsCommercial    ?? project.Details.IsCommercial;
        }

        await _db.SaveChangesAsync(ct);
        return true;
    }
}
