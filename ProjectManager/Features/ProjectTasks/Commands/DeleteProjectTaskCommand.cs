using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Persistence;
using ProjectManager.Services.Interfaces;

namespace ProjectManager.Features.ProjectTasks.Commands;

public record DeleteProjectTaskCommand(Guid TaskId) : IRequest<bool>;

public class DeleteProjectTaskHandler : IRequestHandler<DeleteProjectTaskCommand, bool>
{
    private readonly AppDbContext _db;
    private readonly IUserContext _userContext;
    private readonly IProjectAuthorizationService _auth;

    public DeleteProjectTaskHandler(AppDbContext db, IUserContext userContext, IProjectAuthorizationService auth)
    {
        _db = db;
        _userContext = userContext;
        _auth = auth;
    }

    public async Task<bool> Handle(DeleteProjectTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _db.ProjectTasks
            .Include(t => t.Project)
            .ThenInclude(p => p!.Details)
            .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);

        if (task is null)
            return false;

        var userId = _userContext.UserId;
        var canDelete = await _auth.CanEditProject(userId, task.ProjectId);

        if (!canDelete)
            return false;

        _db.ProjectTasks.Remove(task);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
