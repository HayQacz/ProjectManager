using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Persistence;
using ProjectManager.Services.Interfaces;

namespace ProjectManager.Features.ProjectMembers.Commands;

public record RemoveProjectMemberFromProjectCommand(Guid ProjectId, Guid MemberId, Guid RequestingUserId) : IRequest<bool>;

public class RemoveProjectMemberFromProjectHandler : IRequestHandler<RemoveProjectMemberFromProjectCommand, bool>
{
    private readonly AppDbContext _db;
    private readonly IProjectAuthorizationService _auth;

    public RemoveProjectMemberFromProjectHandler(AppDbContext db, IProjectAuthorizationService auth)
    {
        _db = db;
        _auth = auth;
    }

    public async Task<bool> Handle(RemoveProjectMemberFromProjectCommand request, CancellationToken cancellationToken)
    {
        if (!await _auth.CanManageMembers(request.RequestingUserId, request.ProjectId))
            return false;

        var project = await _db.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project is null)
            return false;

        var member = project.Members.FirstOrDefault(m => m.Id == request.MemberId);
        if (member is null)
            return false;

        project.Members.Remove(member);
        await _db.SaveChangesAsync(cancellationToken);

        return true;
    }
}
