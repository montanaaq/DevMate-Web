using NpgsqlTypes;

namespace SkillIssue.Models.Users;

public enum SocialNetwork
{
    Telegram,

    [PgName("github")]
    GitHub,
}
