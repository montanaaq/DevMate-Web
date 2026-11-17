namespace SkillIssue.Http.Errors;

public sealed class HttpException(
    ApiError error,
    string? message = null,
    Exception? innerException = null
) : Exception(message ?? error.Message, innerException)
{
    public ApiError Error { get; } = error with { Message = message ?? error.Message };
}
