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
        _db = db;
        _userContext = userContext;
    }

    public async Task<bool> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
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

        _db.Projects.Remove(project);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
