namespace ProjectManager.Entities;

public class ProjectCategory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;

    public Guid ProjectDetailsId { get; set; }
    public ProjectDetails ProjectDetails { get; set; } = null!;
    public ICollection<ProjectCategoryItem> Items { get; set; } = new List<ProjectCategoryItem>();
}
