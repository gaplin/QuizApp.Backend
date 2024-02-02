using Microsoft.Extensions.Options;
using MongoDB.Driver;
using QuizApp.Infrastructure.DbModels;
using QuizApp.Infrastructure.DbSettings;
using QuizApp.Infrastructure.Interfaces;

namespace QuizApp.Infrastructure.Context;

internal class QuizAppContext : IQuizAppContext
{
    private readonly IMongoCollection<QuizModel> _quizzes;
    private readonly IMongoCollection<UserModel> _users;

    public QuizAppContext(IMongoDatabase db, IOptions<MongoDbSettings> dbSettings)
    {
        _quizzes = db.GetCollection<QuizModel>(dbSettings.Value.Collections.Quizzes);
        _users = db.GetCollection<UserModel>(dbSettings.Value.Collections.Users);
    }

    IMongoCollection<QuizModel> IQuizAppContext.Quizzes => _quizzes;

    IMongoCollection<UserModel> IQuizAppContext.Users => _users;
}