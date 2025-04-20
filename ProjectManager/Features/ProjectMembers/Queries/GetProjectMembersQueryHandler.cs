using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Persistence;
using ProjectManager.Features.ProjectMembers.Models;
using ProjectManager.Entities;

namespace ProjectManager.Features.ProjectMembers.Queries;

public record GetProjectMembersQuery(Guid ProjectId) : IRequest<List<ProjectMemberDto>>;

public class GetProjectMembersHandler : IRequestHandler<GetProjectMembersQuery, List<ProjectMemberDto>>
{
    private readonly AppDbContext _db;
    public GetProjectMembersHandler(AppDbContext db) => _db = db;

    public async Task<List<ProjectMemberDto>> Handle(GetProjectMembersQuery request, CancellationToken ct)
    {
        var members = await _db.ProjectMembers
            .Where(pm => pm.ProjectId == request.ProjectId)
            .Include(pm => pm.User)
            .ToListAsync(ct);

        return members.Select(pm => new ProjectMemberDto
        {
            Id              = pm.Id,
            Role            = pm.Role,
            UserEmail       = pm.User?.Email,
            UserDisplayName = pm.User?.FullName
        }).ToList();
    }
}
