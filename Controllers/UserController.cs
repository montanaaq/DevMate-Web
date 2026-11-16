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

        var user =
            await authService.AuthenticateAsync(
                request.Email!,
                request.Password!,
                context.CancellationToken
            ) ?? throw new HttpException(ApiError.Unauthorized, ErrorMessages.InvalidCredentials);
    }
}
