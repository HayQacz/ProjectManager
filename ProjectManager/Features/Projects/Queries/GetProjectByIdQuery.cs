using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Features.Projects.Models;
using ProjectManager.Persistence;

namespace ProjectManager.Features.Projects.Queries;

public record GetProjectByIdQuery(Guid Id) : IRequest<ProjectDto?>;

public class GetProjectByIdHandler : IRequestHandler<GetProjectByIdQuery, ProjectDto?>
{
    private readonly AppDbContext _db;

    public GetProjectByIdHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ProjectDto?> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var project = await _db.Projects
            .Include(p => p.Details)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        return project == null ? null : new ProjectDto
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
