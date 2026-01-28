namespace WorkNest.Domain.Entities;

public class Project
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = default!;
    public string OwnerId { get; set; } = default!;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public List<TaskItem> Tasks { get; set; } = new();

    public List<ProjectMember> Members { get; set; } = new();
}
