namespace SkillIssue.Models.Requests;

public sealed class CreateTeamRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public CreateTeamPositionRequest[]? Positions { get; init; }
}

public sealed class CreateTeamPositionRequest
{
    public string? Title { get; init; }
    public int CategoryId { get; init; }
    public int Count { get; init; }
    public bool IncludeCaptain { get; init; }
    public int[]? SkillIds { get; init; }
}
