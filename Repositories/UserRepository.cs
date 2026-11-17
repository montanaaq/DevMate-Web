using SkillIssue.Models.Users;
using Npgsql;

namespace SkillIssue.Repositories;

public interface IUserRepository
{
    Task<int?> CreateAsync(
        string email,
        string fullName,
        string passwordHash,
        CancellationToken cancellationToken
    );

    Task<User?> FindByEmailAsync(string email, CancellationToken ct);

    Task<User?> FindByIdAsync(int userId, CancellationToken cancellationToken);
}

public sealed class UserRepository(NpgsqlDataSource dataSource) : IUserRepository
{
    public async Task<int?> CreateAsync(
        string email,
        string fullName,
        string passwordHash,
        CancellationToken cancellationToken
    )
    {
        await using var command = dataSource.CreateCommand("SELECT public.create_user($1, $2, $3)");
        command.Parameters.AddWithValue(email);
        command.Parameters.AddWithValue(fullName);
        command.Parameters.AddWithValue(passwordHash);

        object? result = await command.ExecuteScalarAsync(cancellationToken);

        return result is null or DBNull ? null : (int)result;
    }

    public async Task<User?> FindByEmailAsync(string email, CancellationToken ct)
    {
        await using var command = dataSource.CreateCommand(
            "SELECT * FROM public.find_user_by_email($1)"
        );
        command.Parameters.AddWithValue(email);

        await using var reader = await command.ExecuteReaderAsync(ct);

        return await ReadUserAsync(reader, ct);
    }

    public async Task<User?> FindByIdAsync(int userId, CancellationToken cancellationToken)
    {
        await using var command = dataSource.CreateCommand(
            "SELECT * FROM public.find_user_by_id($1)"
        );
        command.Parameters.AddWithValue(userId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return await ReadUserAsync(reader, cancellationToken);
    }

    private static async Task<User?> ReadUserAsync(
        NpgsqlDataReader reader,
        CancellationToken cancellationToken
    )
    {
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new User
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            Email = reader.GetString(reader.GetOrdinal("email")),
            FullName = reader.GetString(reader.GetOrdinal("full_name")),
            PasswordHash = reader.GetString(reader.GetOrdinal("password_hash")),
            Role = Enum.Parse<UserRole>(reader.GetString(reader.GetOrdinal("role")), true),
            ProfilePrivate = reader.GetBoolean(reader.GetOrdinal("profile_private")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at")),
        };
    }
}
