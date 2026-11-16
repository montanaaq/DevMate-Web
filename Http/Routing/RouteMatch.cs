using DevMate.Models.Users;

namespace DevMate.Http.Routing;

public sealed record RouteMatch(
    RequestDelegate? Handler,
    IReadOnlyDictionary<string, string> Values,
    IReadOnlyCollection<string> AllowedMethods,
    IReadOnlyCollection<UserRole> AllowedRoles
);
