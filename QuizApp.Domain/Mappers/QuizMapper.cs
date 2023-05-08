using QuizApp.Domain.DTOs;
using QuizApp.Domain.Entities;

namespace QuizApp.Domain.Mappers;

internal static class QuizMapper
{
    internal static Quiz MapToEntity(CreateQuizDTO dto)
        => new()
        {
            Title = dto.Title!,
            Category = dto.Category!,
            Questions = dto.Questions!.Select(QuestionMapper.MapToEntity).ToList(),
            NumberOfQuestions = dto.Questions!.Count,
        };
}