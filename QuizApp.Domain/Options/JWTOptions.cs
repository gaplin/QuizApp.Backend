using System.ComponentModel.DataAnnotations;

namespace QuizApp.Domain.Options;

internal class JwtOptions
{
    [Required]
    public string Issuer { get; set; } = null!;
    [Required]
    public string Audience { get; set; } = null!;
    [Required]
    public string Key { get; set; } = null!;
}