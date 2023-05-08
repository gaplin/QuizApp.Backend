using FluentValidation;
using QuizApp.Domain.DTOs;

namespace QuizApp.Domain.Validators;

internal class CreateQuizDTOValidator : AbstractValidator<CreateQuizDTO>
{
    public CreateQuizDTOValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Category)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Questions)
            .NotNull()
            .Must(x => x!.Count > 0).WithMessage("At least 1 question is required");

        RuleForEach(x => x.Questions)
            .Cascade(CascadeMode.Continue)
            .SetValidator(new CreateQuestionDTOValidator());
    }
}