using QuizApp.Domain.Entities;
using QuizApp.Domain.Interfaces.Repositories;
using QuizApp.Domain.Interfaces.Services;

namespace QuizApp.Domain.Services;

internal class QuizService : IQuizService
{
    private readonly IQuizRepository _repo;

    public QuizService(IQuizRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<Quiz>> GetAsync() =>
        await _repo.GetAsync();

    public async Task<Quiz?> GetAsync(string id) =>
        await _repo.GetAsync(id);

    public async Task InsertAsync(Quiz newQuiz) =>
        await _repo.InsertAsync(newQuiz);

    public async Task UpdateAsync(string id, Quiz updatedQuiz) =>
        await _repo.UpdateAsync(id, updatedQuiz);

    public async Task DeleteAsync(string id) =>
        await _repo.DeleteAsync(id);
}