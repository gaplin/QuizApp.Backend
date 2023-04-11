using MongoDB.Driver;
using QuizApp.Domain.Entities;
using QuizApp.Domain.Interfaces.Repositories;
using QuizApp.Infrastructure.Interfaces;

namespace QuizApp.Infrastructure.Repositories;

internal class QuizRespository : IQuizRepository
{
    private readonly IMongoCollection<Quiz> _quizzes;

    public QuizRespository(IQuizAppContext context)
    {
        _quizzes = context.Quizzes;
    }

    public async Task<List<Quiz>> GetAsync() =>
        await _quizzes.Find(_ => true).ToListAsync();

    public async Task<Quiz?> GetAsync(string id) =>
        await _quizzes.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task InsertAsync(Quiz newQuiz) =>
        await _quizzes.InsertOneAsync(newQuiz);

    public async Task<bool> UpdateAsync(Quiz updatedQuiz)
    {
        var replaceOneResult = await _quizzes.ReplaceOneAsync(x => x.Id == updatedQuiz.Id, updatedQuiz);
        return replaceOneResult.MatchedCount == 1;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var deleteResult = await _quizzes.DeleteOneAsync(x => x.Id == id);
        return deleteResult.DeletedCount == 1;
    }
}