using Microsoft.Extensions.Options;
using MongoDB.Driver;
using QuizApp.Infrastructure.DbModels;
using QuizApp.Infrastructure.DbSettings;
using QuizApp.Infrastructure.Interfaces;

namespace QuizApp.Infrastructure.Context;

internal class QuizAppContext : IQuizAppContext
{
    private readonly IMongoDatabase _db;
    private readonly MongoDbSettings _dbSettings;
    private IMongoCollection<QuizModel>? _quizzes;

    public QuizAppContext(IMongoDatabase db, IOptions<MongoDbSettings> dbSettings)
    {
        _db = db;
        _dbSettings = dbSettings.Value;
    }

    IMongoCollection<QuizModel> IQuizAppContext.Quizzes
    {
        get
        {
            _quizzes ??= _db.GetCollection<QuizModel>(_dbSettings.Collections.Quizzes);
            return _quizzes;
        }
    }
}