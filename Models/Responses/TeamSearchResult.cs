namespace SkillIssue.Models.Responses;

public sealed record TeamSearchResult(
    int Id,
    string Name,
    string Description,
    int MemberCount,
    string[] Positions,
    string[] RequiredSkills
);
