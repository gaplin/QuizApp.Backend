using FluentValidation;
using QuizApp.Domain.DTOs;

namespace QuizApp.Domain.Validators;

internal class CreateQuestionDTOValidator : AbstractValidator<CreateQuestionDTO>
{
    public CreateQuestionDTOValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Text)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Answers)
            .NotNull()
            .Must(x => x!.Count > 1).WithMessage("At least 2 answers required")
            .Must(x => x!.Count < 9).WithMessage("Limit of answers is 8");

        RuleForEach(x => x.Answers)
            .Cascade(CascadeMode.Continue)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.CorrectAnswer)
            .NotNull()
            .Must((question, correctAnswer) => CorrectAnswerValid(question, correctAnswer!.Value))
            .WithMessage((question, _) => CorrectAnswerInvalidMessage(question));
    }

    private static bool CorrectAnswerValid(CreateQuestionDTO question, int correctAnswer)
    {
        var answersCount = question.Answers!.Count;
        if (correctAnswer < 0 || correctAnswer > answersCount - 1)
            return false;
        return true;
    }

    private static string CorrectAnswerInvalidMessage(CreateQuestionDTO question)
    {
        var answersCount = question.Answers!.Count;
        return $"Value must be between 0 and {answersCount - 1} inclusive";
    }
}