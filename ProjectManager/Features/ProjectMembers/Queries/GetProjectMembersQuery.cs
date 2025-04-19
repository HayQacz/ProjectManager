using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Persistence;
using ProjectManager.Features.ProjectMembers.Models;
using ProjectManager.Entities;
using ProjectManager.Entities.Enums;

namespace ProjectManager.Features.ProjectMembers.Queries;

public record GetProjectMembersQuery(Guid ProjectId) : IRequest<List<ProjectMemberDto>>;

public class GetProjectMembersHandler : IRequestHandler<GetProjectMembersQuery, List<ProjectMemberDto>>
{
    private readonly AppDbContext _db;

    public GetProjectMembersHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ProjectMemberDto>> Handle(GetProjectMembersQuery request, CancellationToken cancellationToken)
    {
        var members = await _db.ProjectMembers
            .Where(pm => pm.Projects.Any(p => p.Id == request.ProjectId))
            .Include(pm => pm.User)
            .ToListAsync(cancellationToken);

        return members.Select((ProjectMember pm) => new ProjectMemberDto
        {
            Id = pm.Id,
            Role = pm.Role,
            Email = pm.User?.Email,
            DisplayName = pm.DisplayName
        }).ToList();
    }
}
