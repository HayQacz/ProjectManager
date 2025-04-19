using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Features.Projects.Models;
using ProjectManager.Persistence;
using ProjectManager.Services.Interfaces;
using ProjectManager.Entities.Enums;

namespace ProjectManager.Features.Projects.Queries;

public record GetProjectByIdQuery(Guid Id) : IRequest<ProjectDto?>;

public class GetProjectByIdHandler : IRequestHandler<GetProjectByIdQuery, ProjectDto?>
{
    private readonly AppDbContext _db;
    private readonly IUserContext _userContext;

    public GetProjectByIdHandler(AppDbContext db, IUserContext userContext)
    {
        _db = db;
        _userContext = userContext;
    }

    public async Task<ProjectDto?> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var project = await _db.Projects
            .Include(p => p.Details)
            .Include(p => p.Members)
            .ThenInclude(m => m.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (project == null)
            return null;

        if (project.Details?.Visibility == ProjectVisibility.Private)
        {
            var userId = _userContext.UserId;
            var isMember = project.Members.Any(m => m.User.Id == userId);

            if (!isMember)
                return null;
        }

        return new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            CreatedAt = project.CreatedAt,
            Deadline = project.Details?.Deadline,
            Status = project.Details?.Status,
            TechnologiesUsed = project.Details?.TechnologiesUsed,
            IsCommercial = project.Details?.IsCommercial,
            Visibility = project.Details?.Visibility
        };
    }
}
