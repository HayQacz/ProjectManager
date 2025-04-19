using ProjectManager.Entities.Enums;

namespace ProjectManager.Features.ProjectMembers.Models;


public class ProjectMemberDto
{
    public Guid Id { get; set; }
    public ProjectMemberRole Role { get; set; }
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
}
