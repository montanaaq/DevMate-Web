using System.Net;
using FluentValidation;
using SkillIssue.Http;
using SkillIssue.Http.Errors;
using SkillIssue.Models.Requests;
using SkillIssue.Repositories;
using SkillIssue.Validators;

namespace SkillIssue.Controllers;

public sealed class TeamController(ITeamRepository repository)
{
    private readonly CreateTeamRequestValidator _validator = new();
    private readonly CreateApplicationRequestValidator _applicationValidator = new();

    public async Task CatalogAsync(RequestContext context)
    {
        await context.WriteJsonAsync(await repository.GetCatalogAsync(context.CancellationToken));
    }

    public async Task SearchAsync(RequestContext context)
    {
        string? rawSkills = context.Request.QueryString["skills"];
        string[] values = string.IsNullOrEmpty(rawSkills) ? [] : rawSkills.Split(',');
        if (values.Length > 20)
        {
            throw new HttpException(ApiError.BadRequest);
        }

        var skillIds = new List<int>();
        foreach (string value in values)
        {
            if (!int.TryParse(value, out int id) || id <= 0)
            {
                throw new HttpException(ApiError.BadRequest);
            }
            skillIds.Add(id);
        }

        int beforeId = int.MaxValue;
        string? rawBefore = context.Request.QueryString["before"];
        if (rawBefore is not null && (!int.TryParse(rawBefore, out beforeId) || beforeId <= 0))
        {
            throw new HttpException(ApiError.BadRequest);
        }

        var teams = await repository.SearchAsync(
            skillIds.Distinct().ToArray(),
            beforeId,
            context.User?.Id,
            context.CancellationToken
        );
        bool hasMore = teams.Count > 12;
        await context.WriteJsonAsync(
            new
            {
                teams = teams
                    .Take(12)
                    .Select(team => new
                    {
                        id = team.Id,
                        name = team.Name,
                        description = team.Description,
                        memberCount = team.MemberCount,
                        positions = team.Positions,
                        requiredSkills = team.RequiredSkills,
                        positionIds = team.PositionIds,
                        isMember = team.IsMember,
                        applicationPositions = team.ApplicationPositions,
                    }),
                hasMore,
                isAuthenticated = context.User is not null,
            }
        );
    }

    public async Task CreateAsync(RequestContext context)
    {
        var user = context.User ?? throw new HttpException(ApiError.Unauthorized);
        var request = await context.ReadJsonAsync<CreateTeamRequest>();

        await _validator.ValidateAndThrowAsync(request, context.CancellationToken);
        int teamId =
            await repository.CreateAsync(user.Id, request, context.CancellationToken)
            ?? throw new HttpException(ApiError.BadRequest);

        await context.WriteJsonAsync(new { teamId }, HttpStatusCode.Created);
    }

    public async Task ApplyAsync(RequestContext context)
    {
        var user = context.User ?? throw new HttpException(ApiError.Unauthorized);
        var request = await context.ReadJsonAsync<CreateApplicationRequest>();
        _ = int.TryParse(context.RouteValues["teamId"], out int teamId);
        request.TeamId = teamId;

        await _applicationValidator.ValidateAndThrowAsync(request, context.CancellationToken);

        int applicationId =
            await repository.ApplyAsync(
                request.TeamId,
                request.PositionId,
                user.Id,
                request.CoverLetter?.Trim(),
                context.CancellationToken
            ) ?? throw new HttpException(ApiError.Conflict, ErrorMessages.ApplicationUnavailable);

        await context.WriteJsonAsync(new { applicationId }, HttpStatusCode.Created);
    }
}
