using DevMate.Models.Enums;

namespace DevMate.Models;

public sealed class UserSocialLink
{
    public int Id { get; init; }
    public int UserId { get; init; }
    public required SocialNetwork Network { get; init; }
    public required string Url { get; init; }
}
