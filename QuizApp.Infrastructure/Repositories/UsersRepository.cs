using MongoDB.Driver;
using QuizApp.Domain.Entities;
using QuizApp.Domain.Interfaces.Repositories;
using QuizApp.Infrastructure.DbModels;
using QuizApp.Infrastructure.Interfaces;
using QuizApp.Infrastructure.Mappers;

namespace QuizApp.Infrastructure.Repositories;

internal class UsersRepository : IUsersRepository
{
    private readonly IMongoCollection<UserModel> _users;

    public UsersRepository(IQuizAppContext context)
    {
        _users = context.Users;
    }

    public async Task<List<User>> GetAsync() =>
        (await _users
            .Find(_ => true)
            .ToListAsync())
            .Select(UserMapper.MapToEntity)
            .ToList();

    public async Task<User?> GetAsync(string id) =>
        UserMapper.MapToEntity(
            await _users
                .Find(x => x.Id == id)
                .FirstOrDefaultAsync()
            );

    public async Task<string> InsertAsync(User newUser)
    {
        var model = UserMapper.MapToModel(newUser);
        await _users.InsertOneAsync(model);
        return model.Id!;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var deleteResult = await _users.DeleteOneAsync(x => x.Id == id);
        return deleteResult.DeletedCount == 1;
    }

    public async Task DeleteAsync() =>
        _ = await _users.DeleteManyAsync(_ => true);
}