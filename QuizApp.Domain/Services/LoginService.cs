using FluentValidation;
using QuizApp.Domain.DTOs;
using QuizApp.Domain.Interfaces.Repositories;
using QuizApp.Domain.Interfaces.Services;

namespace QuizApp.Domain.Services;

internal class LoginService : ILoginService
{
    private readonly ITokenService _tokenService;
    private readonly IUsersRepository _usersRepository;
    private readonly IValidator<CredentialsDTO> _credentialsValidator;
    public LoginService(ITokenService tokenService, IUsersRepository usersRepository,
        IValidator<CredentialsDTO> credentialsValidator)
    {
        _tokenService = tokenService;
        _usersRepository = usersRepository;
        _credentialsValidator = credentialsValidator;
    }

    public async Task<(string? token, IDictionary<string, string[]>? validationErrors)> LogInAndGetTokenAsync(CredentialsDTO credentials)
    {
        var validationResult = await _credentialsValidator.ValidateAsync(credentials);
        if (!validationResult.IsValid)
        {
            return (null, validationResult.ToDictionary());
        }
        var user = (await _usersRepository.GetByLoginAsync(credentials.Login!))!;

        var token = _tokenService.GenerateTokenForUser(user);
        return (token, null);
    }
}