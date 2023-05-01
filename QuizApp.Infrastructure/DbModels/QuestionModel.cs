namespace QuizApp.Infrastructure.DbModels;

internal class QuestionModel
{
    public string Text { get; set; } = null!;
    public List<string> Answers { get; set; } = null!;
    public int CorrectAnswer { get; set; }
}