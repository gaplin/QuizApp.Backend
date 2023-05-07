using QuizApp.Domain.DTOs;
using QuizApp.Domain.Enums;

namespace QuizApp.Domain.Interfaces.Services;

public interface IUserService
{
    Task<bool> DeleteAsync(string id);
    Task DeleteAsync();
    Task<IEnumerable<UserDTO>> GetAsync();
    Task<UserDTO?> GetByIdAsync(string id);
    Task<(string? token, IDictionary<string, string[]>? errors)> CreateAsync(CreateUserDTO user);
    Task<EUserType?> GetUserRoleAsync(string id);
}