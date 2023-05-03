using QuizApp.Domain.Entities;

namespace QuizApp.Domain.Interfaces.Services;

public interface IUserService
{
    Task<bool> DeleteAsync(string id);
    Task DeleteAsync();
    Task<List<User>> GetAsync();
    Task<User?> GetAsync(string id);
    Task InsertAsync(User newUser);
}