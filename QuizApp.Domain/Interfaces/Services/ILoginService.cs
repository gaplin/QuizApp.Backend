using QuizApp.Domain.DTOs;

namespace QuizApp.Domain.Interfaces.Services;

public interface ILoginService
{
    Task<(string? token, IDictionary<string, string[]>? validationErrors)> LogInAndGetTokenAsync(CredentialsDTO credentials);
}