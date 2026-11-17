using FluentValidation;
using SkillIssue.Models.Requests;

namespace SkillIssue.Validators;

public sealed class CreateTeamRequestValidator : AbstractValidator<CreateTeamRequest>
{
    public CreateTeamRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(request => request.Name).NotEmpty().MaximumLength(120);
        RuleFor(request => request.Description).NotEmpty().MaximumLength(5000);
        RuleFor(request => request.Positions)
            .NotEmpty()
            .Must(positions =>
                positions?.Length <= 20
                && positions.All(position => position is not null)
                && positions.Sum(position => (long)position.Count) <= 20
                && positions.Count(position => position.IncludeCaptain) <= 1
            );

        RuleForEach(request => request.Positions)
            .NotNull()
            .ChildRules(position =>
            {
                position.RuleFor(value => value.Title).NotEmpty().MaximumLength(120);
                position.RuleFor(value => value.CategoryId).GreaterThan(0);
                position.RuleFor(value => value.Count).InclusiveBetween(1, 20);
                position
                    .RuleFor(value => value.SkillIds)
                    .NotEmpty()
                    .Must(ids =>
                        ids?.Length <= 20
                        && ids.All(id => id > 0)
                        && ids.Distinct().Count() == ids.Length
                    );
            });
    }
}
