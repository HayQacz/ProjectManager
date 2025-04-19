namespace ProjectManager.Entities;

public class ProjectMember
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Role { get; set; } = string.Empty;

    public ICollection<Project> Projects { get; set; } = new List<Project>();

    public User? User { get; set; }
}

