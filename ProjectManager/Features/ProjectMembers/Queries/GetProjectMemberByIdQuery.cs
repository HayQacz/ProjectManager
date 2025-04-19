using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Persistence;
using ProjectManager.Features.ProjectMembers.Models;
using ProjectManager.Entities;
using ProjectManager.Entities.Enums;

namespace ProjectManager.Features.ProjectMembers.Queries;

public record GetProjectMemberByIdQuery(Guid Id) : IRequest<ProjectMemberDto?>;

public class GetProjectMemberByIdHandler : IRequestHandler<GetProjectMemberByIdQuery, ProjectMemberDto?>
{
    private readonly AppDbContext _db;

    public GetProjectMemberByIdHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ProjectMemberDto?> Handle(GetProjectMemberByIdQuery request, CancellationToken cancellationToken)
    {
        var member = await _db.ProjectMembers
            .Include(pm => pm.User)
            .FirstOrDefaultAsync(pm => pm.Id == request.Id, cancellationToken);

        return member == null
            ? null
            : new ProjectMemberDto
            {
                Id = member.Id,
                Role = member.Role,
                Email = member.User?.Email,
                DisplayName = member.DisplayName
            };
    }
}
