using System.Net;
using System.Text.Json;
using DevMate.Http;
using DevMate.Http.Errors;
using DevMate.Models.Requests;
using DevMate.Services;
using DevMate.Validators;
using FluentValidation;

namespace DevMate.Controllers;

public sealed class UserController(IAuthService authService)
{
    private readonly SignUpRequestValidator _signUpValidator = new();
    private readonly SignInRequestValidator _signInValidator = new();

    public async Task SignUpAsync(RequestContext context)
    {
        SignUpRequest? request;

        try
        {
            request = await JsonSerializer.DeserializeAsync<SignUpRequest>(
                context.Request.InputStream,
                JsonSerializerOptions.Web,
                context.CancellationToken
            );
        }
        catch (JsonException exception)
        {
            throw new HttpException(
                ApiError.BadRequest,
                ErrorMessages.InvalidRequestBody,
                exception
            );
        }

        if (request is null)
        {
            throw new HttpException(ApiError.BadRequest, ErrorMessages.InvalidRequestBody);
        }

        await _signUpValidator.ValidateAndThrowAsync(request, context.CancellationToken);

        int userId =
            await authService.RegisterAsync(
                request.Email!,
                request.Password!,
                request.FullName!,
                context.CancellationToken
            ) ?? throw new HttpException(ApiError.Conflict, ErrorMessages.EmailAlreadyExists);

        context.Response.Headers["Cache-Control"] = "no-store";
        await context.WriteJsonAsync(new { userId }, HttpStatusCode.Created);
    }

    public async Task SignInAsync(RequestContext context)
    {
        SignInRequest? request;

        try
        {
            request = await JsonSerializer.DeserializeAsync<SignInRequest>(
                context.Request.InputStream,
                JsonSerializerOptions.Web,
                context.CancellationToken
            );
        }
        catch (JsonException exception)
        {
            throw new HttpException(
                ApiError.BadRequest,
                ErrorMessages.InvalidRequestBody,
                exception
            );
        }

        if (request is null)
        {
            throw new HttpException(ApiError.BadRequest, ErrorMessages.InvalidRequestBody);
        }

        await _signInValidator.ValidateAndThrowAsync(request, context.CancellationToken);

        var result =
            await authService.AuthenticateAsync(
                request.Email!,
                request.Password!,
                context.CancellationToken
            ) ?? throw new HttpException(ApiError.Unauthorized, ErrorMessages.InvalidCredentials);

        SessionCookie.Set(context, result.Token, result.ExpiresAt);
        context.Response.StatusCode = (int)HttpStatusCode.NoContent;
    }

    public async Task SignOutAsync(RequestContext context)
    {
        string? token = context.Request.Cookies[SessionCookie.Name]?.Value;
        await authService.SignOutAsync(token, context.CancellationToken);

        context.Session = null;
        context.User = null;
        SessionCookie.Delete(context);
        context.Response.StatusCode = (int)HttpStatusCode.NoContent;
    }

    public async Task MeAsync(RequestContext context)
    {
        context.Response.Headers["Cache-Control"] = "no-store";

        var user = context.User ?? throw new HttpException(ApiError.Unauthorized);

        await context.WriteJsonAsync(
            new
            {
                userId = user.Id,
                email = user.Email,
                fullName = user.FullName,
                role = user.Role.ToString().ToLowerInvariant(),
            }
        );
    }
}
