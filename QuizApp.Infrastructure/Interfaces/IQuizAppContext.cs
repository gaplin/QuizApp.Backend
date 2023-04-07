using MongoDB.Driver;
using QuizApp.Domain.Entities;

namespace QuizApp.Infrastructure.Interfaces;

internal interface IQuizAppContext
{
    internal IMongoCollection<Quiz> Quizzes { get; }
}