using QuizApp.Domain.Enums;

namespace QuizApp.Domain.DTOs;

public class UserDTO
{
    public required string Id { get; set; }
    public required string UserName { get; set; }
    public required EUserType UserType { get; set; }
}