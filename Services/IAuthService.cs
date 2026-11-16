using DevMate.Models.Users;

namespace DevMate.Services;

public interface IAuthService
{
    Task<User?> AuthenticateAsync(
        string email,
        string password,
        CancellationToken cancellationToken
    );
}
