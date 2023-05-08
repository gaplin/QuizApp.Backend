namespace QuizApp.Domain.DTOs;

public class CreateQuestionDTO
{
    public string? Text { get; set; }
    public List<string>? Answers { get; set; }
    public int? CorrectAnswer { get; set; }
}