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

    public async Task<List<QuizBase>> GetBaseInfoAsync() =>
        await _repo.GetBaseAsync();

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

    public async Task InsertAsync(Quiz newQuiz)
    {
        var id = await _repo.InsertAsync(newQuiz);
        newQuiz.Id = id;
    }

    public async Task<bool> DeleteAsync(string id) =>
        await _repo.DeleteAsync(id);

    public async Task DeleteAsync() =>
        await _repo.DeleteAsync();

    public async Task<List<Quiz>> GetAsync() =>
        await _repo.GetAsync();
}