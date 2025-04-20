using ProjectManager.Entities.Enums;

namespace ProjectManager.Entities;

public class ProjectMember
{
    public Guid Id            { get; set; } = Guid.NewGuid();
    public Guid ProjectId     { get; set; }            
    public Guid UserId        { get; set; }           
    public string? DisplayName{ get; set; }
    public ProjectMemberRole  Role { get; set; } = ProjectMemberRole.Viewer;

    public Project? Project   { get; set; }
    public User?    User      { get; set; }
}
