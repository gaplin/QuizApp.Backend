namespace QuizApp.Domain.Entities;

public class Question
{
    public string Text { get; set; } = null!;
    public IList<string> CorrectAnswers { get; set; } = null!;
    public IList<string> WrongAnswers { get; set; } = null!;
}