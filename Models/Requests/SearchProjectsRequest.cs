namespace SkillIssue.Models.Requests;

public sealed record SearchProjectsRequest(int[]? SkillIds, int? Before);
