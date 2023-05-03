using QuizApp.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuizApp.Domain.Entities;

public class User
{
    public string? Id { get; set; }
    [MinLength(5)]
    [MaxLength(20)]
    public string UserName { get; set; } = null!;
    [MinLength(5)]
    [MaxLength(20)]
    public string Login { get; set; } = null!;
    public string HPassword { get; set; } = null!;
    public EUserType UserType { get; set; }
}