using FluentValidation;
using QuizApp.Domain.DTOs;
using QuizApp.Domain.Interfaces.Repositories;
using QuizApp.Domain.Interfaces.Services;

namespace QuizApp.Domain.Validators;

internal class CredentialsDTOValidator : AbstractValidator<CredentialsDTO>
{
    private readonly IUsersRepository _usersRepository;
    private readonly IHashService _hashService;
    public CredentialsDTOValidator(IUsersRepository usersRepository, IHashService hashService)
    {
        _usersRepository = usersRepository;
        _hashService = hashService;

        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(credentials => credentials.Login)
            .NotEmpty();

        RuleFor(credentials => credentials.Password)
            .NotEmpty();

        RuleFor(credentials => credentials)
            .MustAsync(LoginAndPasswordCorrectAsync).WithName("Login,Password").WithMessage("Invalid Login or Password");
    }

    private async Task<bool> LoginAndPasswordCorrectAsync(CredentialsDTO credentials, CancellationToken token)
    {
        var user = await _usersRepository.GetByLoginAsync(credentials.Login!);
        if (user is null) return false;

        var hash = user.HPassword;
        var hashMatch = _hashService.VerifyPassword(credentials.Password!, hash);
        return hashMatch;
    }
}