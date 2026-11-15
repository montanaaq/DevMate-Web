namespace DevMate.Shared.Constants;

public static class ErrorMessages
{
    public const string PageNotFound = "Страница не найдена.";
    public const string FileNotFound = "Файл не найден.";
    public const string MethodNotAllowed = "Метод запроса не поддерживается.";
    public const string RequestFailed = "Не удалось обработать запрос. Попробуйте позже.";
    public const string ConnectionClosed = "Клиентское соединение закрыто до завершения ответа.";
    public const string RequestProcessingFailed = "Ошибка HTTP-запроса";
    public const string ResponseWriteFailed = "Не удалось завершить HTTP-запрос";
    public const string ResponseCloseFailed = "Не удалось закрыть HTTP-ответ";
    public const string InvalidUrl = "Неверный url";
    public const string InvalidRouteTemplate =
        "Шаблон маршрута должен начинаться с / и не содержать ? или #.";
    public const string DuplicateRoute = "Маршрут уже зарегистрирован.";
}
