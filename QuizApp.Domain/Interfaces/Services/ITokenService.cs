using QuizApp.Domain.Entities;

namespace QuizApp.Domain.Interfaces.Services;

internal interface ITokenService
{
    string GenerateTokenForUser(User user);
}