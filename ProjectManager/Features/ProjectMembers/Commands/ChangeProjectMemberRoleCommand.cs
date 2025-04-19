using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Persistence;
using ProjectManager.Entities.Enums;
using ProjectManager.Features.ProjectMembers.Models;

namespace ProjectManager.Features.ProjectMembers.Commands;

public record ChangeProjectMemberRoleCommand(Guid MemberId, ProjectMemberRole NewRole) : IRequest<ProjectMemberDto?>;

public class ChangeProjectMemberRoleHandler : IRequestHandler<ChangeProjectMemberRoleCommand, ProjectMemberDto?>
{
    private readonly AppDbContext _db;

    public ChangeProjectMemberRoleHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ProjectMemberDto?> Handle(ChangeProjectMemberRoleCommand request, CancellationToken cancellationToken)
    {
        var member = await _db.ProjectMembers
            .Include(pm => pm.User)
            .FirstOrDefaultAsync(pm => pm.Id == request.MemberId, cancellationToken);

        if (member is null)
            return null;

        member.Role = request.NewRole;
        await _db.SaveChangesAsync(cancellationToken);

        return new ProjectMemberDto
        {
            Id = member.Id,
            Role = member.Role,
            Email = member.User?.Email,
            DisplayName = member.DisplayName
        };
    }
}
