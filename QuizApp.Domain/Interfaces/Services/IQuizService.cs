using QuizApp.Domain.Entities;
using System.Security.Claims;

namespace QuizApp.Domain.Interfaces.Services;

public interface IQuizService
{
    Task<bool> DeleteAsync(string id);
    Task DeleteAsync();
    Task<List<QuizBase>> GetBaseInfoAsync();
    Task<List<Quiz>> GetAsync();
    Task<Quiz?> GetAsync(string id, bool shuffle);
    Task InsertAsync(Quiz newQuiz, ClaimsPrincipal claims);
}