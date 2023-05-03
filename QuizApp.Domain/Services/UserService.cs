using QuizApp.Domain.Entities;
using QuizApp.Domain.Interfaces.Repositories;
using QuizApp.Domain.Interfaces.Services;

namespace QuizApp.Domain.Services;

public class UserService : IUserService
{
    private readonly IUsersRepository _repo;

    public UserService(IUsersRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<User>> GetAsync() =>
    await _repo.GetAsync();

    public async Task<User?> GetAsync(string id) =>
        await _repo.GetAsync(id);

    public async Task InsertAsync(User newUser)
    {
        var id = await _repo.InsertAsync(newUser);
        newUser.Id = id;
    }

    public async Task<bool> DeleteAsync(string id) =>
        await _repo.DeleteAsync(id);

    public async Task DeleteAsync() =>
        await _repo.DeleteAsync();
}