using QuizApp.Domain.Entities;

namespace QuizApp.Domain.Interfaces.Repositories;

public interface IQuizRepository
{
    Task<bool> DeleteAsync(string id);
    Task<IList<Quiz>> GetAsync();
    Task<Quiz?> GetAsync(string id);
    Task InsertAsync(Quiz newQuiz);
    Task<bool> UpdateAsync(Quiz updatedQuiz);
}