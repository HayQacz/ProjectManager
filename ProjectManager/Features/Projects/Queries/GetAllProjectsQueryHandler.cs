using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Features.Projects.Models;
using ProjectManager.Persistence;
using ProjectManager.Services.Interfaces;
using ProjectManager.Entities.Enums;

namespace ProjectManager.Features.Projects.Queries;

public record GetAllProjectsQuery : IRequest<List<ProjectDto>>;

public class GetAllProjectsHandler : IRequestHandler<GetAllProjectsQuery, List<ProjectDto>>
{
    private readonly AppDbContext _db;
    private readonly IUserContext _userContext;

    public GetAllProjectsHandler(AppDbContext db, IUserContext userContext)
    {
        _db = db;
        _userContext = userContext;
    }

    public async Task<List<ProjectDto>> Handle(GetAllProjectsQuery request, CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId;

        var visibleProjects = await _db.Projects
            .Include(p => p.Details)
            .Include(p => p.Members)
            .ThenInclude(m => m.User)
            .Where(p =>
                p.Details!.Visibility != ProjectVisibility.Private ||
                p.Members.Any(m => m.User.Id == userId)
            )
            .AsNoTracking()
            .Select(p => new ProjectDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                CreatedAt = p.CreatedAt,
                Deadline = p.Details!.Deadline,
                Status = p.Details.Status,
                TechnologiesUsed = p.Details.TechnologiesUsed,
                IsCommercial = p.Details.IsCommercial,
                Visibility = p.Details.Visibility
            })
            .ToListAsync(cancellationToken);

        return visibleProjects;
    }
}
