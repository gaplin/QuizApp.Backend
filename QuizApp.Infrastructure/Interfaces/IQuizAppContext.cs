using MongoDB.Driver;
using QuizApp.Domain.Entities;

namespace QuizApp.Infrastructure.Interfaces;

internal interface IQuizAppContext
{
    IMongoCollection<Quiz> Quizzes { get; }
}