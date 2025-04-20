using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Persistence;
using ProjectManager.Services.Interfaces;

namespace ProjectManager.Features.Projects.Commands;

public record DeleteProjectCommand(Guid Id) : IRequest<bool>;

public class DeleteProjectHandler : IRequestHandler<DeleteProjectCommand, bool>
{
    private readonly AppDbContext                _db;
    private readonly IUserContext                _user;
    private readonly IProjectAuthorizationService _auth;

    public DeleteProjectHandler(
        AppDbContext db,
        IUserContext userContext,
        IProjectAuthorizationService auth)
    {
        _db   = db;
        _user = userContext;
        _auth = auth;
    }

    public async Task<bool> Handle(DeleteProjectCommand req, CancellationToken ct)
    {
        if (!await _auth.CanDeleteProject(_user.UserId, req.Id))
            throw new UnauthorizedAccessException("You do not have permission to delete project.");

        var project = await _db.Projects
            .FirstOrDefaultAsync(p => p.Id == req.Id, ct);

        if (project is null) throw new KeyNotFoundException("Project not found.");

        _db.Projects.Remove(project);          
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
