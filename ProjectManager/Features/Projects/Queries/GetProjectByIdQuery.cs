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
    private readonly IProjectAuthorizationService _authService;

    public GetProjectByIdHandler(AppDbContext db, IUserContext userContext, IProjectAuthorizationService authService)
    {
        _db = db;
        _userContext = userContext;
        _authService = authService;
    }

    public async Task<ProjectDto?> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var project = await _db.Projects
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (project is null)
            return null;

        var userId = _userContext.UserId;

        if (!await _authService.CanViewProject(userId, request.Id))
            return null;

        return new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            CreatedAt = project.CreatedAt,
            Deadline = project.Details?.Deadline,
            TechnologiesUsed = project.Details?.TechnologiesUsed,
            Status = project.Details?.Status,
            Visibility = project.Details?.Visibility,
            IsCommercial = project.Details?.IsCommercial
        };
    }
}

