﻿namespace ProjectManager.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
    public Guid? ProjectMemberId { get; set; }
}
