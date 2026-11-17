using System.Net;
using FluentValidation;
using SkillIssue.Http;
using SkillIssue.Http.Errors;
using SkillIssue.Models.Requests;
using SkillIssue.Repositories;
using SkillIssue.Services;
using SkillIssue.Validators;

namespace SkillIssue.Controllers;

public sealed class UserController(IAuthService authService, IUserRepository repository)
{
    private readonly SignUpRequestValidator _signUpValidator = new();
    private readonly SignInRequestValidator _signInValidator = new();
    private readonly UpdateSocialLinksRequestValidator _socialLinksValidator = new();

    public async Task ProfileAsync(RequestContext context)
    {
        var user = context.User ?? throw new HttpException(ApiError.Unauthorized);
        context.Response.Headers["Cache-Control"] = "no-store";
        await context.WriteJsonAsync(
            await repository.GetProfileAsync(user.Id, context.CancellationToken)
        );
    }

    public async Task SaveSocialLinksAsync(RequestContext context)
    {
        var user = context.User ?? throw new HttpException(ApiError.Unauthorized);
        var request = await context.ReadJsonAsync<UpdateSocialLinksRequest>();

        await _socialLinksValidator.ValidateAndThrowAsync(request, context.CancellationToken);

        await repository.SaveSocialLinksAsync(user.Id, request, context.CancellationToken);
        context.Response.Headers["Cache-Control"] = "no-store";
        context.Response.StatusCode = (int)HttpStatusCode.NoContent;
    }

    public async Task SignUpAsync(RequestContext context)
    {
        var request = await context.ReadJsonAsync<SignUpRequest>();

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
        var request = await context.ReadJsonAsync<SignInRequest>();

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
