namespace QuizApp.Domain.DTOs;

public class CreateQuizDTO
{
    public string? Title { get; set; }
    public string? Category { get; set; }
    public List<CreateQuestionDTO>? Questions { get; set; }
}