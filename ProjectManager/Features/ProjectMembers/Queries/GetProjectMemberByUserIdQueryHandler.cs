using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Persistence;
using ProjectManager.Features.ProjectMembers.Models;

namespace ProjectManager.Features.ProjectMembers.Queries;

public record GetProjectMemberByUserIdQuery(Guid ProjectId, Guid UserId) : IRequest<ProjectMemberDto?>;

public class GetProjectMemberByUserIdHandler : IRequestHandler<GetProjectMemberByUserIdQuery, ProjectMemberDto?>
{
    private readonly AppDbContext _db;
    public GetProjectMemberByUserIdHandler(AppDbContext db) => _db = db;

    public async Task<ProjectMemberDto?> Handle(GetProjectMemberByUserIdQuery request, CancellationToken ct)
    {
        var member = await _db.ProjectMembers
            .Include(pm => pm.User)
            .FirstOrDefaultAsync(pm =>
                pm.ProjectId == request.ProjectId &&
                pm.UserId    == request.UserId, ct);

        return member is null ? null : new ProjectMemberDto
        {
            Id              = member.Id,
            Role            = member.Role,
            UserEmail       = member.User?.Email,
            UserDisplayName = member.User?.FullName
        };
    }
}
