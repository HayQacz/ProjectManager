using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Persistence;
using ProjectManager.Services.Interfaces;
using ProjectManager.Entities.Enums;

namespace ProjectManager.Features.Projects.Commands;

public record DeleteProjectCommand(Guid Id) : IRequest<bool>;

public class DeleteProjectHandler : IRequestHandler<DeleteProjectCommand, bool>
{
    private readonly AppDbContext _db;
    private readonly IUserContext _userContext;

    public DeleteProjectHandler(AppDbContext db, IUserContext userContext)
    {
        _db          = db;
        _userContext = userContext;
    }

    public async Task<bool> Handle(DeleteProjectCommand request, CancellationToken ct)
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

        _db.Projects.Remove(project);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
