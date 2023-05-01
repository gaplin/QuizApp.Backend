namespace QuizApp.Domain.Entities;

public class Quiz : QuizBase
{
    public List<Question> Questions { get; set; } = null!;
}