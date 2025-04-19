namespace ProjectManager.Entities;

public class ProjectCategoryItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }

    public Guid CategoryId { get; set; }
    public ProjectCategory Category { get; set; } = null!;
}
