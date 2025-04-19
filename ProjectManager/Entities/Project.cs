namespace ProjectManager.Entities;

public class Project
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; private set; }
    public string Description { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public ProjectDetails? Details { get; set; }
    public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();

    private Project() { }

    public Project(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public void Update(string name, string description)
    {
        Name = name;
        Description = description;
    }
}
