using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Persistence;
using ProjectManager.Entities;
using ProjectManager.Services.Interfaces;

namespace ProjectManager.Features.ProjectMembers.Commands;

public record AddProjectMemberToProjectCommand(Guid ProjectId, Guid MemberId, Guid RequestingUserId) : IRequest<bool>;

public class AddProjectMemberToProjectHandler : IRequestHandler<AddProjectMemberToProjectCommand, bool>
{
    private readonly AppDbContext _db;
    private readonly IProjectAuthorizationService _auth;

    public AddProjectMemberToProjectHandler(AppDbContext db, IProjectAuthorizationService auth)
    {
        _db = db;
        _auth = auth;
    }

    public async Task<bool> Handle(AddProjectMemberToProjectCommand request, CancellationToken cancellationToken)
    {
        if (!await _auth.CanManageMembers(request.RequestingUserId, request.ProjectId))
            return false;

        var project = await _db.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);
        var member = await _db.ProjectMembers.FirstOrDefaultAsync(m => m.Id == request.MemberId, cancellationToken);

        if (project is null || member is null)
            return false;

        if (!project.Members.Contains(member))
        {
            project.Members.Add(member);
            await _db.SaveChangesAsync(cancellationToken);
        }

        return true;
    }
}
