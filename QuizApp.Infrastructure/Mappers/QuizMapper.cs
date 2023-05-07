using QuizApp.Domain.Entities;
using QuizApp.Infrastructure.DbModels;

namespace QuizApp.Infrastructure.Mappers;

internal static class QuizMapper
{
    internal static QuizModel MapToModel(Quiz entity) => new()
    {
        Id = entity.Id,
        Title = entity.Title,
        Author = entity.Author,
        AuthorId = entity.AuthorId,
        Category = entity.Category,
        Questions = MapQuestions(entity.Questions)
    };

    private static List<QuestionModel> MapQuestions(List<Question> entities) =>
        entities.Select(QuestionMapper.Map).ToList();
}