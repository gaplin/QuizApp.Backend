using MongoDB.Driver;
using QuizApp.Domain.Entities;
using QuizApp.Domain.Interfaces.Repositories;
using QuizApp.Infrastructure.DbModels;
using QuizApp.Infrastructure.Interfaces;
using QuizApp.Infrastructure.Mappers;
using QuizApp.Infrastructure.Projections;

namespace QuizApp.Infrastructure.Repositories;

internal class QuizRespository : IQuizRepository
{
    private readonly IMongoCollection<QuizModel> _quizzes;

    public QuizRespository(IQuizAppContext context)
    {
        _quizzes = context.Quizzes;
    }

    public async Task<List<QuizBase>> GetBaseAsync() =>
        await _quizzes
            .Find(_ => true)
            .Project(QuizProjections.ModelToBaseEntityProjection())
            .ToListAsync();

    public async Task<List<Quiz>> GetAsync() =>
        await _quizzes
            .Find(_ => true)
            .Project(QuizProjections.ModelToEntityProjection())
            .ToListAsync();

    public async Task<Quiz?> GetAsync(string id) =>
            await _quizzes.Find(x => x.Id == id)
            .Project(QuizProjections.ModelToEntityProjection())
            .FirstOrDefaultAsync();

    public async Task<string> InsertAsync(Quiz newQuiz)
    {
        var model = QuizMapper.MapToModel(newQuiz);
        await _quizzes.InsertOneAsync(model);
        return model.Id!;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var deleteResult = await _quizzes.DeleteOneAsync(x => x.Id == id);
        return deleteResult.DeletedCount == 1;
    }

    public async Task DeleteAsync() =>
        _ = await _quizzes.DeleteManyAsync(_ => true);
}