namespace SkillIssue.Models.Users;

public sealed class UserSkill
{
    public int UserId { get; init; }
    public int SkillId { get; init; }
    public short Level { get; init; }
}
