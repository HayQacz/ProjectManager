using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Persistence;

namespace ProjectManager.Features.ProjectMembers.Commands;

public record RemoveProjectMemberFromProjectCommand(Guid ProjectId, Guid MemberId) : IRequest<bool>;

public class RemoveProjectMemberFromProjectHandler : IRequestHandler<RemoveProjectMemberFromProjectCommand, bool>
{
    private readonly AppDbContext _db;

    public RemoveProjectMemberFromProjectHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(RemoveProjectMemberFromProjectCommand request, CancellationToken cancellationToken)
    {
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
