using MediatR;
using ProjectManager.Entities;
using ProjectManager.Entities.Enums;
using ProjectManager.Features.ProjectTasks.Models;
using ProjectManager.Persistence;
using ProjectManager.Services.Interfaces;

namespace ProjectManager.Features.ProjectTasks.Commands;

public record CreateProjectTaskCommand(
    Guid ProjectId,
    string Title,
    string? Description,
    DateTime? Deadline,
    Guid? AssignedMemberId
) : IRequest<ProjectTaskDto>;

public class CreateProjectTaskHandler : IRequestHandler<CreateProjectTaskCommand, ProjectTaskDto>
{
    private readonly AppDbContext _db;
    private readonly IUserContext _userContext;
    private readonly IProjectAuthorizationService _auth;

    public CreateProjectTaskHandler(AppDbContext db, IUserContext userContext, IProjectAuthorizationService auth)
    {
        _db = db;
        _userContext = userContext;
        _auth = auth;
    }

    public async Task<ProjectTaskDto> Handle(CreateProjectTaskCommand request, CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId;
        var canEdit = await _auth.CanEditProject(userId, request.ProjectId);

        if (!canEdit)
            throw new UnauthorizedAccessException("You do not have permission to add tasks to this project.");

        var task = new ProjectTask
        {
            Title = request.Title,
            Description = request.Description,
            Deadline = request.Deadline,
            ProjectId = request.ProjectId,
            AssignedMemberId = request.AssignedMemberId
        };

        _db.ProjectTasks.Add(task);
        await _db.SaveChangesAsync(cancellationToken);

        return new ProjectTaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Deadline = task.Deadline,
            Status = task.Status,
            AssignedMemberId = task.AssignedMemberId,
            ProjectId = task.ProjectId
        };
    }
}
