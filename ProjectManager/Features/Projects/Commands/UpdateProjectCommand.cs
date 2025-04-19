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
    DateTime? Deadline = null,
    string? TechnologiesUsed = null,
    ProjectStatus? Status = null,
    ProjectVisibility? Visibility = null,
    bool? IsCommercial = null
) : IRequest<bool>;

public class UpdateProjectHandler : IRequestHandler<UpdateProjectCommand, bool>
{
    private readonly AppDbContext _db;
    private readonly IUserContext _userContext;

    public UpdateProjectHandler(AppDbContext db, IUserContext userContext)
    {
        _db = db;
        _userContext = userContext;
    }

    public async Task<bool> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _db.Projects
            .Include(p => p.Details)
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (project is null)
            return false;

        var userId = _userContext.UserId;

        var member = await _db.ProjectMembers
            .Include(m => m.User)
            .Include(m => m.Projects)
            .FirstOrDefaultAsync(m => m.User.Id == userId && m.Projects.Any(p => p.Id == request.Id), cancellationToken);

        if (member == null || (member.Role != ProjectMemberRole.Owner && member.Role != ProjectMemberRole.Manager))
            return false;

        project.Update(request.Name, request.Description);

        if (project.Details is not null)
        {
            project.Details.Deadline = request.Deadline ?? project.Details.Deadline;
            project.Details.Status = request.Status ?? project.Details.Status;
            project.Details.TechnologiesUsed = request.TechnologiesUsed ?? project.Details.TechnologiesUsed;
            project.Details.Visibility = request.Visibility ?? project.Details.Visibility;
            project.Details.IsCommercial = request.IsCommercial ?? project.Details.IsCommercial;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
