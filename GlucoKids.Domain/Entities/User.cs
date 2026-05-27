namespace GlucoKids.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string FirebaseUid { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public UserRole Role { get; set; } = UserRole.Child;
    public bool IsAdmin { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Child? Child { get; set; }
}

public enum UserRole { Child }
