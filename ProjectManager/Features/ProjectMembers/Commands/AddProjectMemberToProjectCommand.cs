using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Persistence;
using ProjectManager.Entities;

namespace ProjectManager.Features.ProjectMembers.Commands;

public record AddProjectMemberToProjectCommand(Guid ProjectId, Guid MemberId) : IRequest<bool>;

public class AddProjectMemberToProjectHandler : IRequestHandler<AddProjectMemberToProjectCommand, bool>
{
    private readonly AppDbContext _db;

    public AddProjectMemberToProjectHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(AddProjectMemberToProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _db.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        var member = await _db.ProjectMembers
            .FirstOrDefaultAsync(m => m.Id == request.MemberId, cancellationToken);

        if (project is null || member is null)
            return false;

        if (project.Members.Any(m => m.Id == request.MemberId))
            return true; 

        project.Members.Add(member);
        await _db.SaveChangesAsync(cancellationToken);

        return true;
    }
}
