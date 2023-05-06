namespace QuizApp.Domain.DTOs;

public class CreateUserDTO
{
    public string? Login { get; set; }
    public string? Password { get; set; }
    public string? UserName { get; set; }
}