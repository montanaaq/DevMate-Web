using System.Net;
using System.Text.Json;
using SkillIssue.Http.Errors;
using FluentValidation;

namespace SkillIssue.Http.Middleware;

public static class ErrorMiddleware
{
    public static async Task InvokeAsync(RequestContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpException exception) when (!context.HasStarted)
        {
            await context.WriteErrorAsync(exception.Error);
        }
        catch (ValidationException exception) when (!context.HasStarted)
        {
            if (!context.IsApiRequest)
            {
                await context.WriteErrorAsync(ApiError.BadRequest, ErrorMessages.ValidationFailed);
                return;
            }

            await context.WriteJsonAsync(
                new
                {
                    code = ApiError.BadRequest.Code,
                    message = ErrorMessages.ValidationFailed,
                    errors = exception.Errors.Select(error => new
                    {
                        field = JsonNamingPolicy.CamelCase.ConvertName(error.PropertyName),
                        code = error.ErrorCode,
                        message = error.ErrorCode == "required"
                            ? ErrorMessages.RequiredField
                            : ErrorMessages.InvalidField,
                    }),
                },
                ApiError.BadRequest.StatusCode
            );
        }
        catch (ObjectDisposedException exception)
            when (exception.ObjectName == typeof(HttpListenerResponse).FullName)
        {
            Console.Error.WriteLine(ErrorMessages.ConnectionClosed);
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"{ErrorMessages.RequestProcessingFailed}: {exception}");

            if (context.HasStarted)
            {
                context.Response.Abort();
                return;
            }

            await context.WriteErrorAsync(ApiError.InternalServerError);
        }
    }
}
