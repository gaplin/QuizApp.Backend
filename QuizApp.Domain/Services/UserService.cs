using FluentValidation;
using QuizApp.Domain.DTOs;
using QuizApp.Domain.Entities;
using QuizApp.Domain.Enums;
using QuizApp.Domain.Interfaces.Repositories;
using QuizApp.Domain.Interfaces.Services;
using QuizApp.Domain.Mappers;

namespace QuizApp.Domain.Services;

internal class UserService : IUserService
{
    private readonly IUsersRepository _repo;
    private readonly IHashService _hashService;
    private readonly IValidator<CreateUserDTO> _createUserValidator;

    public UserService(IUsersRepository repo, IHashService hashService,
        IValidator<CreateUserDTO> createUserValidator)
    {
        _repo = repo;
        _hashService = hashService;
        _createUserValidator = createUserValidator;
    }

    public async Task<IEnumerable<UserDTO>> GetAsync() =>
    (await _repo.GetAsync())
        .Select(UserMapper.MapEntityToDto);

    public async Task<UserDTO?> GetByIdAsync(string id) =>
        await _repo.GetByIdAsync(id)
        is User user
        ? UserMapper.MapEntityToDto(user)
        : null;

    public async Task<EUserType?> GetUserRoleAsync(string id)
    {
        var user = await _repo.GetByIdAsync(id);
        return user?.UserType;
    }

    public async Task<IDictionary<string, string[]>?> CreateAsync(CreateUserDTO user)
    {
        var validationResult = await _createUserValidator.ValidateAsync(user);
        if (!validationResult.IsValid)
        {
            return validationResult.ToDictionary();
        }
        var passwordHash = _hashService.PasswordHash(user.Password!);
        var userEntity = new User
        {
            HPassword = passwordHash,
            Login = user.Login!,
            UserName = user.UserName!,
            UserType = EUserType.User
        };
        _ = await _repo.InsertAsync(userEntity);

        return null;
    }

    public async Task<bool> DeleteAsync(string id) =>
        await _repo.DeleteAsync(id);

    public async Task DeleteAsync() =>
        await _repo.DeleteAsync();
}