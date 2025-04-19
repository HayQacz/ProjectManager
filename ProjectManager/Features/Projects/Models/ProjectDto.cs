using ProjectManager.Entities.Enums;

namespace ProjectManager.Features.Projects.Models;

public class ProjectDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public DateTime? Deadline { get; set; }
    public ProjectStatus? Status { get; set; }
    public string? TechnologiesUsed { get; set; }
    public bool? IsCommercial { get; set; }
    public ProjectVisibility? Visibility { get; set; }
}
