using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Features.Projects.Models;
using ProjectManager.Persistence;

namespace ProjectManager.Features.Projects.Queries;

public record GetAllProjectsQuery : IRequest<List<ProjectDto>>;

public class GetAllProjectsHandler : IRequestHandler<GetAllProjectsQuery, List<ProjectDto>>
{
    private readonly AppDbContext _db;

    public GetAllProjectsHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ProjectDto>> Handle(GetAllProjectsQuery request, CancellationToken cancellationToken)
    {
        return await _db.Projects
            .Include(p => p.Details)
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
    }
}
