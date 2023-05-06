using FluentValidation;
using QuizApp.Domain.DTOs;
using QuizApp.Domain.Interfaces.Repositories;

namespace QuizApp.Domain.Validators;

public class CreateUserDTOValidator : AbstractValidator<CreateUserDTO>
{
    private readonly IUsersRepository _usersRepository;
    public CreateUserDTOValidator(IUsersRepository usersRepository)
    {
        _usersRepository = usersRepository;

        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(user => user.Login)
            .NotEmpty()
            .MinimumLength(5)
            .MaximumLength(20)
            .MustAsync(LoginUniqueAsync).WithMessage("Login already taken");

        RuleFor(user => user.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(20);

        RuleFor(user => user.UserName)
            .NotEmpty()
            .MinimumLength(5)
            .MaximumLength(20);
    }

    private async Task<bool> LoginUniqueAsync(string login, CancellationToken token)
    {
        var user = await _usersRepository.GetByLoginAsync(login);
        return user is null;
    }
}