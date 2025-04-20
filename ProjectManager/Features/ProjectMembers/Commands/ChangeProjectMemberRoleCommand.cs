using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Entities;
using ProjectManager.Entities.Enums;
using ProjectManager.Persistence;
using ProjectManager.Services.Interfaces;
using ProjectManager.Features.ProjectMembers.Models;

namespace ProjectManager.Features.ProjectMembers.Commands;

public record ChangeProjectMemberRoleCommand(Guid? UserId, ProjectMemberRole NewRole, Guid RequestingUserId, Guid ProjectId) : IRequest<ProjectMemberDto?>;

public class ChangeProjectMemberRoleHandler : IRequestHandler<ChangeProjectMemberRoleCommand, ProjectMemberDto?>
{
    private readonly AppDbContext _db;
    private readonly IProjectAuthorizationService _auth;

    public ChangeProjectMemberRoleHandler(AppDbContext db, IProjectAuthorizationService auth)
    {
        _db = db;
        _auth = auth;
    }

    public async Task<ProjectMemberDto?> Handle(ChangeProjectMemberRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var canManage = await _auth.CanManageMembers(request.RequestingUserId, request.ProjectId);
            if (!canManage)
                throw new UnauthorizedAccessException("You do not have permission to change member roles.");

            var member = await _db.ProjectMembers
                .Include(pm => pm.User)
                .FirstOrDefaultAsync(pm => pm.UserId == request.UserId && pm.ProjectId == request.ProjectId, cancellationToken);

            if (member == null)
                throw new KeyNotFoundException("The project member was not found.");

            member.Role = request.NewRole;
            await _db.SaveChangesAsync(cancellationToken);

            return new ProjectMemberDto
            {
                Id = member.Id,
                Role = member.Role,
                UserEmail = member.User?.Email,
                UserDisplayName = member.User?.FullName
            };
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new UnauthorizedAccessException("You do not have permission to change member roles.", ex);
        }
        catch (KeyNotFoundException ex)
        {
            throw new KeyNotFoundException("The project member was not found.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred while changing the member role.", ex);
        }
    }
}