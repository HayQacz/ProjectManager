using ProjectManager.Entities.Enums;

namespace ProjectManager.Entities;

public class ProjectDetails
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Deadline { get; set; }
    public ProjectStatus Status { get; set; }
    public string TechnologiesUsed { get; set; } = string.Empty;
    public ProjectVisibility Visibility { get; set; }
    public bool IsCommercial { get; set; }

    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public ICollection<ProjectCategory> Categories { get; set; } = new List<ProjectCategory>();
}
