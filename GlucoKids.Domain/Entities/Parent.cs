namespace GlucoKids.Domain.Entities;

public class Parent
{
    public int Id { get; set; }
    public string? Phone { get; set; }

    public User User { get; set; } = null!;
}

public class ParentChildLink
{
    public int Id { get; set; }
    public int ParentId { get; set; }
    public int ChildId { get; set; }
    public string InviteCode { get; set; } = string.Empty;
    public LinkStatus Status { get; set; } = LinkStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LinkedAt { get; set; }

    public User ParentUser { get; set; } = null!;
    public User ChildUser { get; set; } = null!;
}

public enum LinkStatus { Pending, Active }
