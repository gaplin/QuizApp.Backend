namespace QuizApp.Domain.Entities;

public class QuizBase
{
    public string? Id { get; set; }
    public string Title { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string Author { get; set; } = null!;
    public int NumberOfQuestions { get; set; }
}