using ProjectManager.Entities.Enums;

namespace ProjectManager.Features.ProjectTasks.Models;

public class ProjectTaskDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? Deadline { get; set; }
    public ProjectTaskStatus Status { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? AssignedMemberId { get; set; }
}
