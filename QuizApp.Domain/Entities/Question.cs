namespace QuizApp.Domain.Entities;

public class Question
{
    public string Text { get; set; } = null!;
    public IList<string> Answers { get; set; } = null!;
    public int CorrectAnswer { get; set; }
}