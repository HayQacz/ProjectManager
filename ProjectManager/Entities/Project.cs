namespace ProjectManager.Entities;

public class Project
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public ProjectDetails? Details { get; set; }

    public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();

    public Project(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public Project(string name, string description, ProjectDetails details) : this(name, description)
    {
        Details = details;
    }

    public void Update(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public void AddMember(ProjectMember member)
    {
        if (!Members.Contains(member))
            Members.Add(member);
    }

    public void RemoveMember(ProjectMember member)
    {
        if (Members.Contains(member))
            Members.Remove(member);
    }
}
