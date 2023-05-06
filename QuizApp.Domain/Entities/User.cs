using QuizApp.Domain.Enums;

namespace QuizApp.Domain.Entities;

public class User
{
    public string? Id { get; set; }
    public string UserName { get; set; } = null!;
    public string Login { get; set; } = null!;
    public string HPassword { get; set; } = null!;
    public EUserType UserType { get; set; }
}