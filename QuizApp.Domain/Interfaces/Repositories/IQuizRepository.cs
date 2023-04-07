using QuizApp.Domain.Entities;

namespace QuizApp.Domain.Interfaces.Repositories;

public interface IQuizRepository
{
    Task DeleteAsync(string id);
    Task<List<Quiz>> GetAsync();
    Task<Quiz?> GetAsync(string id);
    Task InsertAsync(Quiz newQuiz);
    Task UpdateAsync(string id, Quiz updatedQuiz);
}