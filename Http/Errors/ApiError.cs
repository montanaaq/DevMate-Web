using System.Net;

namespace DevMate.Http.Errors;

public sealed record ApiError(HttpStatusCode StatusCode, string Code, string Message)
{
    public static readonly ApiError BadRequest = new(
        HttpStatusCode.BadRequest,
        "bad_request",
        "Некорректный запрос."
    );

    public static readonly ApiError Unauthorized = new(
        HttpStatusCode.Unauthorized,
        "unauthorized",
        "Необходимо войти в аккаунт."
    );

    public static readonly ApiError Forbidden = new(
        HttpStatusCode.Forbidden,
        "forbidden",
        "Недостаточно прав для выполнения действия."
    );

    public static readonly ApiError NotFound = new(
        HttpStatusCode.NotFound,
        "not_found",
        "Ресурс не найден."
    );

    public static readonly ApiError MethodNotAllowed = new(
        HttpStatusCode.MethodNotAllowed,
        "method_not_allowed",
        "Метод запроса не поддерживается."
    );

    public static readonly ApiError Conflict = new(
        HttpStatusCode.Conflict,
        "conflict",
        "Запрос конфликтует с текущим состоянием ресурса."
    );

    public static readonly ApiError InternalServerError = new(
        HttpStatusCode.InternalServerError,
        "internal_server_error",
        "Не удалось обработать запрос. Попробуйте позже."
    );
}
