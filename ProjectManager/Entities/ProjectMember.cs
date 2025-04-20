using ProjectManager.Entities.Enums;

namespace ProjectManager.Entities;

public class ProjectMember
{
    public Guid  Id        { get; init; } = Guid.NewGuid();

    public Guid  ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public Guid  UserId    { get; set; }
    public User  User      { get; set; } = null!;

    public string?           DisplayName { get; set; }
    public ProjectMemberRole Role        { get; set; } = ProjectMemberRole.Viewer;
}

