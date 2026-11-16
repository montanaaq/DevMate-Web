using System.Text.Json.Serialization;

namespace DevMate.Models.Users;

public sealed class User
{
    public int Id { get; init; }
    public required string Email { get; init; }
    public required string FullName { get; init; }

    [JsonIgnore]
    public string PasswordHash { get; init; } = string.Empty;
    public UserRole Role { get; init; } = UserRole.User;
    public bool ProfilePrivate { get; init; } = true;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
