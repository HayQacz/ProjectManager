using ProjectManager.Entities.Enums;

namespace ProjectManager.Entities;

public class ProjectMember
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string DisplayName { get; set; } = string.Empty;

    public ProjectMemberRole Role { get; set; } = ProjectMemberRole.Viewer;
    
    public ICollection<Project> Projects { get; set; } = new List<Project>();

    public User? User { get; set; }


    
}

