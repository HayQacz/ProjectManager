using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Persistence;
using ProjectManager.Features.ProjectMembers.Models;

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
        var member = await _db.ProjectMembers.FindAsync(new object?[] { request.Id }, cancellationToken);
        return member == null
            ? null
            : new ProjectMemberDto
            {
                Id = member.Id,
                Role = member.Role
            };
    }
}
