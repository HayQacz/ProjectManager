using MediatR;
using ProjectManager.Entities;
using ProjectManager.Entities.Enums;
using ProjectManager.Persistence;
using ProjectManager.Features.ProjectMembers.Models;

namespace ProjectManager.Features.ProjectMembers.Commands;

public record CreateProjectMemberCommand(ProjectMemberRole Role) : IRequest<ProjectMemberDto>;

public class CreateProjectMemberHandler : IRequestHandler<CreateProjectMemberCommand, ProjectMemberDto>
{
    private readonly AppDbContext _db;

    public CreateProjectMemberHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ProjectMemberDto> Handle(CreateProjectMemberCommand request, CancellationToken cancellationToken)
    {
        var member = new ProjectMember
        {
            Role = request.Role
        };

        _db.ProjectMembers.Add(member);
        await _db.SaveChangesAsync(cancellationToken);

        return new ProjectMemberDto
        {
            Id = member.Id,
            Role = member.Role
        };
    }
}
