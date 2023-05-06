using System.ComponentModel.DataAnnotations;

namespace QuizApp.Domain.DTOs;

public class CreateUserDTO
{
    [MinLength(5)]
    [MaxLength(20)]
    public string Login { get; set; } = null!;
    [MinLength(8)]
    [MaxLength(20)]
    public string Password { get; set; } = null!;
    [MinLength(5)]
    [MaxLength(20)]
    public string UserName { get; set; } = null!;
}