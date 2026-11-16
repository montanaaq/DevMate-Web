using DevMate.Models.Users;
using DevMate.Repositories;

namespace DevMate.Services;

public sealed class AuthService(UserRepository userRepository) : IAuthService
{
    public async Task<User?> AuthenticateAsync(
        string email,
        string password,
        CancellationToken cancellationToken
    )
    {
        var user = await userRepository.FindByEmailAsync(email.Trim(), cancellationToken);

        if (user is null)
        {
            return null;
        }

        if (!PasswordHasher.Verify(password, user.PasswordHash))
        {
            return null;
        }

        return user;
    }
}
