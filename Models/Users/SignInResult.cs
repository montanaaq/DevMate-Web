using System.Text.Json.Serialization;

namespace DevMate.Models.Users;

public sealed record SignInResult(
    User User,
    [property: JsonIgnore] string Token,
    DateTime ExpiresAt
);
