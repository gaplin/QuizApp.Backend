using MongoDB.Driver;
using QuizApp.Infrastructure.DbModels;

namespace QuizApp.Infrastructure.Interfaces;

internal interface IQuizAppContext
{
    internal IMongoCollection<QuizModel> Quizzes { get; }
    internal IMongoCollection<UserModel> Users { get; }
}