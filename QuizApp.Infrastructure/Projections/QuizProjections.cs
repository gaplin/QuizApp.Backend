using MongoDB.Driver;
using QuizApp.Domain.Entities;
using QuizApp.Infrastructure.DbModels;

namespace QuizApp.Infrastructure.Projections;

internal static class QuizProjections
{
    internal static ProjectionDefinition<QuizModel, QuizBase> ModelToBaseEntityProjection()
        => Builders<QuizModel>.Projection.Expression(model => new QuizBase
        {
            Id = model.Id,
            Author = model.Author,
            Category = model.Category,
            Title = model.Title,
        });

    internal static ProjectionDefinition<QuizModel, Quiz> ModelToEntityProjection()
        => Builders<QuizModel>.Projection.Expression(model => new Quiz
        {
            Id = model.Id,
            Author = model.Author,
            Category = model.Category,
            Title = model.Title,
            Questions = model.Questions.Select(x => new Question()
            {
                Answers = x.Answers,
                CorrectAnswer = x.CorrectAnswer,
                Text = x.Text,
            }).ToList()
        });
}