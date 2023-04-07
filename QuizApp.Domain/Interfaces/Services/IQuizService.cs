using QuizApp.Domain.Entities;

namespace QuizApp.Domain.Interfaces.Services;

public interface IQuizService
{
    Task DeleteAsync(string id);
    Task<List<Quiz>> GetAsync();
    Task<Quiz?> GetAsync(string id);
    Task InsertAsync(Quiz newQuiz);
    Task UpdateAsync(string id, Quiz updatedQuiz);
}