using QuizApp.Domain.Entities;

namespace QuizApp.Domain.Interfaces.Repositories;

public interface IUsersRepository
{
    Task<bool> DeleteAsync(string id);
    Task DeleteAsync();
    Task<List<User>> GetAsync();
    Task<User?> GetAsync(string id);
    Task<string> InsertAsync(User newUser);
}