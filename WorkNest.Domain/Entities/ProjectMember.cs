namespace WorkNest.Domain.Entities;

public enum ProjectRole
{
    Owner = 1,
    Member = 2
}

public class ProjectMember
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = default!;

    public string UserId { get; set; } = default!;

    public ProjectRole Role { get; set; } = ProjectRole.Member;

    public DateTime JoinedAtUtc { get; set; } = DateTime.UtcNow;
}
