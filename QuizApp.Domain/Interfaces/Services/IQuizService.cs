using QuizApp.Domain.Entities;

namespace QuizApp.Domain.Interfaces.Services;

public interface IQuizService
{
    Task<bool> DeleteAsync(string id);
    Task DeleteAsync();
    Task<IList<Quiz>> GetAsync();
    Task<Quiz?> GetAsync(string id, bool shuffle);
    Task InsertAsync(Quiz newQuiz);
    Task<bool> UpdateAsync(Quiz updatedQuiz);
}