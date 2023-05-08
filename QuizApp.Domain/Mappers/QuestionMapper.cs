using QuizApp.Domain.DTOs;
using QuizApp.Domain.Entities;

namespace QuizApp.Domain.Mappers;

internal static class QuestionMapper
{
    internal static Question MapToEntity(CreateQuestionDTO dto)
        => new()
        {
            Text = dto.Text!,
            Answers = dto.Answers!,
            CorrectAnswer = dto.CorrectAnswer!.Value
        };
}