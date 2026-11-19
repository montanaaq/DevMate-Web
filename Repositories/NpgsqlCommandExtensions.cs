using Npgsql;

namespace SkillIssue.Repositories;

internal static class NpgsqlCommandExtensions
{
    public static async Task<T?> ExecuteNullableScalarAsync<T>(
        this NpgsqlCommand command,
        CancellationToken cancellationToken
    )
        where T : struct
    {
        object? result = await command.ExecuteScalarAsync(cancellationToken);
        return result is null or DBNull ? null : (T)result;
    }
}
