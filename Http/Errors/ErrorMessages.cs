namespace DevMate.Http.Errors;

public static class ErrorMessages
{
    public const string ValidationFailed = "Check the highlighted fields.";
    public const string RequiredField = "This field is required.";
    public const string InvalidField = "Enter a valid value.";
    public const string ConnectionClosed = "The client disconnected before the response completed.";
    public const string RequestProcessingFailed = "HTTP request failed";
    public const string ResponseWriteFailed = "Failed to complete the HTTP response";
    public const string ResponseCloseFailed = "Failed to close the HTTP response";
    public const string InvalidRouteTemplate =
        "Route templates must start with / and cannot contain ? or #.";
    public const string DuplicateRoute = "The route is already registered.";
    public const string InvalidCredentials = "Incorrect email or password.";
    public const string EmailAlreadyExists = "This email is already registered.";
}
