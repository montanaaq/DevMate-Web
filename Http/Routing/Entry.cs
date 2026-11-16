using DevMate.Models.Users;

namespace DevMate.Http.Routing;

public sealed record Entry(
    string Method,
    string[] Segments,
    string Pattern,
    RequestDelegate Handler,
    IReadOnlyCollection<UserRole> AllowedRoles
);
