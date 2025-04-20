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
    DateTime? Deadline           = null,
    string? TechnologiesUsed     = null,
    ProjectStatus? Status        = null,
    ProjectVisibility? Visibility = null,
    bool? IsCommercial           = null
) : IRequest<bool>;

public class UpdateProjectHandler : IRequestHandler<UpdateProjectCommand, bool>
{
    private readonly AppDbContext                _db;
    private readonly IUserContext                _user;
    private readonly IProjectAuthorizationService _auth;

    public UpdateProjectHandler(
        AppDbContext db,
        IUserContext userContext,
        IProjectAuthorizationService auth)
    {
        _db   = db;
        _user = userContext;
        _auth = auth;
    }

    public async Task<bool> Handle(UpdateProjectCommand req, CancellationToken ct)
    {
        if (!await _auth.CanEditProject(_user.UserId, req.Id))
            throw new UnauthorizedAccessException("You do not have permission to delete project.");

        var project = await _db.Projects
                               .Include(p => p.Details)
                               .FirstOrDefaultAsync(p => p.Id == req.Id, ct);
        if (project is null) throw new KeyNotFoundException("Project not found.");

        project.Update(req.Name, req.Description);

        if (project.Details is not null)
        {
            project.Details.Deadline         = req.Deadline        ?? project.Details.Deadline;
            project.Details.Status           = req.Status          ?? project.Details.Status;
            project.Details.TechnologiesUsed = req.TechnologiesUsed?? project.Details.TechnologiesUsed;
            project.Details.Visibility       = req.Visibility      ?? project.Details.Visibility;
            project.Details.IsCommercial     = req.IsCommercial    ?? project.Details.IsCommercial;
        }

        await _db.SaveChangesAsync(ct);
        return true;
    }
}
