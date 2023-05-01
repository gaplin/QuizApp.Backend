using QuizApp.Domain.Entities;
using QuizApp.Domain.Interfaces.Randomizers;
using QuizApp.Domain.Interfaces.Repositories;
using QuizApp.Domain.Interfaces.Services;

namespace QuizApp.Domain.Services;

internal class QuizService : IQuizService
{
    private readonly IQuizRepository _repo;
    private readonly IQuizRandomizer _quizRandomizer;

    public QuizService(IQuizRepository repo, IQuizRandomizer quizRandomizer)
    {
        _repo = repo;
        _quizRandomizer = quizRandomizer;
    }

    public async Task<IList<Quiz>> GetAsync() =>
        await _repo.GetAsync();

    public async Task<Quiz?> GetAsync(string id, bool shuffle)
    {
        var quiz = await _repo.GetAsync(id);
        if (quiz is null || !shuffle)
        {
            return quiz;
        }

        _quizRandomizer.Randomize(quiz);
        return quiz;
    }

    public async Task InsertAsync(Quiz newQuiz) =>
        await _repo.InsertAsync(newQuiz);

    public async Task<bool> UpdateAsync(Quiz updatedQuiz) =>
        await _repo.UpdateAsync(updatedQuiz);

    public async Task<bool> DeleteAsync(string id) =>
        await _repo.DeleteAsync(id);

    public async Task DeleteAsync() =>
        await _repo.DeleteAsync();
}