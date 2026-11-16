using DevMate.Models.Users;
using Npgsql;

namespace DevMate.Repositories;

public sealed class UserRepository(NpgsqlDataSource dataSource)
{
    public async Task<User?> FindByEmailAsync(string email, CancellationToken ct)
    {
        await using var command = dataSource.CreateCommand(
            "SELECT * FROM public.find_user_by_email($1)"
        );
        command.Parameters.AddWithValue(email);

        await using var reader = await command.ExecuteReaderAsync(ct);

        if (!await reader.ReadAsync(ct))
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
