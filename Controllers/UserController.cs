using System.Text.Json;
using DevMate.Http;
using DevMate.Http.Errors;
using DevMate.Models.Requests;
using DevMate.Validators;
using FluentValidation;
using Npgsql;

namespace DevMate.Controllers;

public sealed class UserController(NpgsqlDataSource dataSource)
{
    private readonly SignUpRequestValidator _signUpValidator = new();

    public NpgsqlDataSource DataSource { get; } = dataSource;

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
}
