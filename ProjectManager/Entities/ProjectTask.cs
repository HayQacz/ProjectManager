using ProjectManager.Entities.Enums;

namespace ProjectManager.Entities;

public class ProjectTask
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProjectTaskStatus Status { get; set; } = ProjectTaskStatus.ToDo;
    public DateTime? Deadline { get; set; }

    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public Guid? AssignedMemberId { get; set; }
    public ProjectMember? AssignedMember { get; set; }
}
