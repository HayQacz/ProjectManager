using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Features.ProjectTasks.Models;
using ProjectManager.Persistence;
using ProjectManager.Services.Interfaces;
using ProjectManager.Entities.Enums;

namespace ProjectManager.Features.ProjectTasks.Queries;

public record GetProjectTasksQuery(
    Guid ProjectId,
    ProjectTaskStatus? Status = null,
    bool OnlyAssignedToMe     = false,
    bool OnlyUnassigned       = false
) : IRequest<List<ProjectTaskDto>>;

public class GetProjectTasksHandler : IRequestHandler<GetProjectTasksQuery, List<ProjectTaskDto>>
{
    private readonly AppDbContext _db;
    private readonly IUserContext _userContext;

    public GetProjectTasksHandler(AppDbContext db, IUserContext userContext)
    {
        _db          = db;
        _userContext = userContext;
    }

    public async Task<List<ProjectTaskDto>> Handle(GetProjectTasksQuery request, CancellationToken ct)
    {
        var member = await _db.ProjectMembers
                              .FirstOrDefaultAsync(pm =>
                                   pm.ProjectId == request.ProjectId &&
                                   pm.UserId    == _userContext.UserId, ct);

        var allowedRoles = new[] { ProjectMemberRole.Contributor, ProjectMemberRole.Manager, ProjectMemberRole.Owner };

        if (member is null || !allowedRoles.Contains(member.Role))
            throw new UnauthorizedAccessException("You are not authorized to view project tasks.");

        var query = _db.ProjectTasks.AsQueryable()
                     .Where(t => t.ProjectId == request.ProjectId);

        if (request.Status.HasValue)
            query = query.Where(t => t.Status == request.Status);

        if (request.OnlyAssignedToMe)
            query = query.Where(t => t.AssignedMemberId == member.Id);

        if (request.OnlyUnassigned)
            query = query.Where(t => t.AssignedMemberId == null);

        return await query.Select(t => new ProjectTaskDto
        {
            Id               = t.Id,
            Title            = t.Title,
            Description      = t.Description,
            Deadline         = t.Deadline,
            Status           = t.Status,
            ProjectId        = t.ProjectId,
            AssignedMemberId = t.AssignedMemberId
        }).ToListAsync(ct);
    }
}
