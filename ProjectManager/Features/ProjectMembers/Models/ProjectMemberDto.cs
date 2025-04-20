using ProjectManager.Entities.Enums;
using System;

namespace ProjectManager.Features.ProjectMembers.Models;

public class ProjectMemberDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? UserDisplayName { get; set; }
    public ProjectMemberRole Role { get; set; }
}