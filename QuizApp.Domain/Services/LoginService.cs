using QuizApp.Domain.Entities;
using QuizApp.Domain.Interfaces.Services;

namespace QuizApp.Domain.Services;

internal class LoginService : ILoginService
{
    private readonly ITokenService _tokenService;

    public LoginService(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public string? LogInAndGetToken()
    {
        var user = new User()
        {
            UserName = "UserName",
        };
        return _tokenService.GenerateTokenForUser(user);
    }
}