using System.Text.Json;

namespace SkillIssue.Models.Responses;

public sealed record TeamSearchResult(
    int Id,
    string Name,
    string Description,
    int MemberCount,
    string[] Positions,
    string[] RequiredSkills,
    int[] PositionIds,
    bool IsMember,
    JsonElement ApplicationPositions,
    bool HasPendingApplication
);
