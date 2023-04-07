using System.ComponentModel.DataAnnotations;

namespace QuizApp.Infrastructure.DbSettings;

internal class CollectionNamesSettings
{
    [Required]
    public string Quizzes { get; set; } = null!;
}