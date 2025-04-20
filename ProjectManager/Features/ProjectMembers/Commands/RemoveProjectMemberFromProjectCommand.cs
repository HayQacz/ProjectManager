using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Entities;
using ProjectManager.Persistence;
using ProjectManager.Services.Interfaces;

namespace ProjectManager.Features.ProjectMembers.Commands
{
    public record RemoveProjectMemberFromProjectCommand(Guid ProjectId, Guid? UserId, Guid RequestingUserId) : IRequest<bool>;

    public class RemoveProjectMemberFromProjectHandler : IRequestHandler<RemoveProjectMemberFromProjectCommand, bool>
    {
        private readonly AppDbContext _db;
        private readonly IProjectAuthorizationService _auth;

        public RemoveProjectMemberFromProjectHandler(AppDbContext db, IProjectAuthorizationService auth)
        {
            _db = db;
            _auth = auth;
        }

        public async Task<bool> Handle(RemoveProjectMemberFromProjectCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (!await _auth.CanManageMembers(request.RequestingUserId, request.ProjectId))
                    throw new UnauthorizedAccessException("You do not have permission to remove members.");

                var project = await _db.Projects
                    .Include(p => p.Members)
                    .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

                if (project == null)
                    throw new KeyNotFoundException("The project was not found.");

                var member = project.Members.FirstOrDefault(m => m.UserId == request.UserId);
                if (member == null)
                    throw new KeyNotFoundException("The project member was not found.");

                project.Members.Remove(member);
                await _db.SaveChangesAsync(cancellationToken);

                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new UnauthorizedAccessException("You do not have permission to remove members.", ex);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("The project member was not found.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while removing the member from the project.", ex);
            }
        }
    }
}
