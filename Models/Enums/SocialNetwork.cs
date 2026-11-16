using NpgsqlTypes;

namespace DevMate.Models.Enums;

public enum SocialNetwork
{
    Telegram,

    [PgName("github")]
    GitHub,
}
