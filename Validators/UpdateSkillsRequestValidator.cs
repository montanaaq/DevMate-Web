using FluentValidation;

namespace SkillIssue.Validators;

public sealed class UpdateSkillsRequestValidator : AbstractValidator<int[]>
{
    public UpdateSkillsRequestValidator()
    {
        RuleFor(skillIds => skillIds)
            .Must(skillIds =>
                skillIds.Length <= 20
                && skillIds.All(id => id > 0)
                && skillIds.Distinct().Count() == skillIds.Length
            );
    }
}
