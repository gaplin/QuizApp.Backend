using QuizApp.Domain.Entities;

namespace QuizApp.Domain.Interfaces.Services;

public interface IQuizService
{
    Task<bool> DeleteAsync(string id);
    Task DeleteAsync();
    Task<IList<Quiz>> GetAsync();
    Task<Quiz?> GetAsync(string id);
    Task<IList<Quiz>> GetInRandomOrderAsync();
    Task InsertAsync(Quiz newQuiz);
    Task<bool> UpdateAsync(Quiz updatedQuiz);
}