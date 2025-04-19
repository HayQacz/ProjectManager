namespace ProjectManager.Entities;

public class ProjectMember
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserEmail { get; set; } = string.Empty;

    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
}
