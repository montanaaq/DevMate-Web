using NpgsqlTypes;

namespace DevMate.Models.Users;

public enum SocialNetwork
{
    Telegram,

    [PgName("github")]
    GitHub,
}
