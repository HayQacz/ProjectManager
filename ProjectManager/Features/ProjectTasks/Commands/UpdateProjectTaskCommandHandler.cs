using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Persistence;
using ProjectManager.Services.Interfaces;
using ProjectManager.Features.ProjectTasks.Models;
using ProjectManager.Entities.Enums;

namespace ProjectManager.Features.ProjectTasks.Commands;

public record UpdateProjectTaskCommand(
    Guid Id,
    string Title,
    string? Description,
    DateTime? Deadline,
    ProjectTaskStatus Status,
    Guid ProjectId,
    Guid? AssignedMemberId
) : IRequest<bool>;

public class UpdateProjectTaskHandler : IRequestHandler<UpdateProjectTaskCommand, bool>
{
    private readonly AppDbContext _db;
    private readonly IUserContext _userContext;
    private readonly IProjectAuthorizationService _auth;

    public UpdateProjectTaskHandler(AppDbContext db, IUserContext userContext, IProjectAuthorizationService auth)
    {
        _db = db;
        _userContext = userContext;
        _auth = auth;
    }

    public async Task<bool> Handle(UpdateProjectTaskCommand request, CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId;
        var canEdit = await _auth.CanEditProject(userId, request.ProjectId);

        if (!canEdit)
            return false;

        var task = await _db.ProjectTasks.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
        if (task is null)
            return false;

        task.Title = request.Title;
        task.Description = request.Description;
        task.Deadline = request.Deadline;
        task.Status = request.Status;
        task.AssignedMemberId = request.AssignedMemberId;

        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
