using Microsoft.Extensions.Caching.Memory;
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
    private readonly IMemoryCache _memoryCache;

    public UsersRepository(IQuizAppContext context, IMemoryCache memoryCache)
    {
        _users = context.Users;
        _memoryCache = memoryCache;
    }

    public async Task<List<User>> GetAsync() =>
        (await _users
            .Find(_ => true)
            .ToListAsync())
            .Select(UserMapper.MapToEntity)
            .ToList();

    public async Task<User?> GetByIdAsync(string id)
    {
        var user = await _memoryCache.GetOrCreateAsync(
            $"userById_{id}",
            cacheEntry =>
            {
                cacheEntry.SlidingExpiration = TimeSpan.FromSeconds(5);
                return _users
                        .Find(x => x.Id == id)
                        .FirstOrDefaultAsync();
            });
        if (user != null)
        {
            return UserMapper.MapToEntity(user);
        }
        return null;
    }

    public async Task<User?> GetByLoginAsync(string login)
    {
        var user = await _memoryCache.GetOrCreateAsync(
            $"userByLogin_{login}",
            cacheEntry =>
            {
                cacheEntry.SlidingExpiration = TimeSpan.FromSeconds(5);
                return _users
                        .Find(x => x.Login == login)
                        .FirstOrDefaultAsync();
            });
        if (user != null)
        {
            return UserMapper.MapToEntity(user);
        }
        return null;
    }

    public async Task<string> InsertAsync(User newUser)
    {
        var model = UserMapper.MapToModel(newUser);
        await _users.InsertOneAsync(model);
        return model.Id!;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var deleteResult = await _users.DeleteOneAsync(x => x.Id == id);
        var deletedCount = deleteResult.DeletedCount;
        if (deletedCount != 0)
        {
            ClearCache();
            return true;
        }
        return false;
    }

    public async Task DeleteAsync()
    {
        _ = await _users.DeleteManyAsync(_ => true);
        ClearCache();
    }

    private void ClearCache()
    {
        ((MemoryCache)_memoryCache).Clear();
    }
}