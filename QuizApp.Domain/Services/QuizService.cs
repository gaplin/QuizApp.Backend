using FluentValidation;
using Microsoft.IdentityModel.JsonWebTokens;
using QuizApp.Domain.DTOs;
using QuizApp.Domain.Entities;
using QuizApp.Domain.Interfaces.Randomizers;
using QuizApp.Domain.Interfaces.Repositories;
using QuizApp.Domain.Interfaces.Services;
using QuizApp.Domain.Mappers;
using System.Security.Claims;

namespace QuizApp.Domain.Services;

internal class QuizService : IQuizService
{
    private readonly IQuizRepository _repo;
    private readonly IQuizRandomizer _quizRandomizer;
    private readonly IValidator<CreateQuizDTO> _createQuizValidator;

    public QuizService(IQuizRepository repo, IQuizRandomizer quizRandomizer,
        IValidator<CreateQuizDTO> createQuizValidator)
    {
        _repo = repo;
        _quizRandomizer = quizRandomizer;
        _createQuizValidator = createQuizValidator;
    }

    public async Task<List<QuizBase>> GetBaseInfoAsync() =>
        await _repo.GetBaseAsync();

    public async Task<Quiz?> GetAsync(string id, bool shuffle)
    {
        var quiz = await _repo.GetAsync(id);
        if (quiz is null || !shuffle)
        {
            return quiz;
        }

        _quizRandomizer.Randomize(quiz);
        return quiz;
    }

    public async Task<IDictionary<string, string[]>?> InsertAsync(CreateQuizDTO newQuiz, ClaimsPrincipal claims)
    {
        var validationResult = await _createQuizValidator.ValidateAsync(newQuiz);
        if (!validationResult.IsValid)
        {
            return validationResult.ToDictionary();
        }
        var quizEntity = QuizMapper.MapToEntity(newQuiz);
        var idClaim = claims.FindFirst(nameof(User.Id));
        var nameClaim = claims.FindFirst(JwtRegisteredClaimNames.Name);

        var creatorId = idClaim!.Value;
        var creatorName = nameClaim!.Value;

        quizEntity.Author = creatorName;
        quizEntity.AuthorId = creatorId;

        _ = await _repo.InsertAsync(quizEntity);

        return null;
    }

    public async Task<bool> DeleteAsync(string id) =>
        await _repo.DeleteAsync(id);

    public async Task DeleteAsync() =>
        await _repo.DeleteAsync();

    public async Task<List<Quiz>> GetAsync() =>
        await _repo.GetAsync();
}