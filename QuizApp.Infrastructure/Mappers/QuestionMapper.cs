using QuizApp.Domain.Entities;
using QuizApp.Infrastructure.DbModels;

namespace QuizApp.Infrastructure.Mappers;

internal static class QuestionMapper
{
    internal static QuestionModel Map(Question entity) => new()
    {
        Answers = entity.Answers,
        CorrectAnswer = entity.CorrectAnswer,
        Text = entity.Text
    };
}