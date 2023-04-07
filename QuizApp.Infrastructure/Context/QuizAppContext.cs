using Microsoft.Extensions.Options;
using MongoDB.Driver;
using QuizApp.Domain.Entities;
using QuizApp.Infrastructure.DbSettings;
using QuizApp.Infrastructure.Interfaces;

namespace QuizApp.Infrastructure.Context;

internal class QuizAppContext : IQuizAppContext
{
    private readonly IMongoDatabase _db;
    private readonly MongoDbSettings _dbSettings;
    private IMongoCollection<Quiz>? _quizzes;

    public QuizAppContext(IMongoDatabase db, IOptions<MongoDbSettings> dbSettings)
    {
        _db = db;
        _dbSettings = dbSettings.Value;
    }

    public IMongoCollection<Quiz> Quizzes
    {
        get
        {
            _quizzes ??= _db.GetCollection<Quiz>(_dbSettings.Collections.Quizzes);
            return _quizzes;
        }
    }
}