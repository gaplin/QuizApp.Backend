using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using QuizApp.Domain.Entities;
using QuizApp.Infrastructure.DbModels;
using QuizApp.Infrastructure.Interfaces;
using QuizApp.Infrastructure.Mappers;

namespace QuizApp.Tests.TestsUtils;

internal static class DbUtilities
{
    internal static async Task<User> CreateRandomUserAsync(IServiceProvider serviceProvider, EUserTypeModel userType)
    {
        var userModel = RandomDataProvider.UserModel(userType);
        using var scope = serviceProvider.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<IQuizAppContext>();
        var users = db.Users;

        await users.InsertOneAsync(userModel);
        var user = UserMapper.MapToEntity(userModel);
        return user;
    }

    internal static async Task<List<UserModel>> GetAllUsersAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<IQuizAppContext>();
        var usersCollection = db.Users;

        var users = await usersCollection.Find(_ => true).ToListAsync();

        return users;
    }

    internal static async Task<List<QuizModel>> GetAllQuizzesAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<IQuizAppContext>();
        var quizzes = db.Quizzes;

        var result = await quizzes.Find(_ => true).ToListAsync();
        return result;
    }

    internal static async Task<List<QuizModel>> CreateRandomQuizzesAsync(IServiceProvider serviceProvider, int count)
    {
        var result = RandomDataProvider.QuizModels(count);
        using var scope = serviceProvider.CreateAsyncScope();
        var db = serviceProvider.GetRequiredService<IQuizAppContext>();
        var quizzesCollection = db.Quizzes;
        await quizzesCollection.InsertManyAsync(result);

        return result;
    }

    internal static async Task<QuizModel> CreateRandomQuizAsync(IServiceProvider serviceProvider, string authorName, string authorId, int numOfQuestions)
    {
        var quizModel = RandomDataProvider.QuizModel(authorName, authorId, numOfQuestions);
        using var scope = serviceProvider.CreateAsyncScope();

        var db = serviceProvider.GetRequiredService<IQuizAppContext>();
        var quizzes = db.Quizzes;

        await quizzes.InsertOneAsync(quizModel);
        return quizModel;
    }

    internal static async Task DeleteAllQuizzesAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<IQuizAppContext>();
        var quizzes = db.Quizzes;

        await quizzes.DeleteManyAsync(_ => true);
    }

    internal static async Task DeleteAllUsersAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<IQuizAppContext>();
        MemoryCache cache = (MemoryCache)scope.ServiceProvider.GetRequiredService<IMemoryCache>();
        var users = db.Users;

        await users.DeleteManyAsync(_ => true);
        cache.Clear();
    }

    internal static async Task DeleteAllAsync(IServiceProvider serviceProvider)
    {
        await DeleteAllQuizzesAsync(serviceProvider);
        await DeleteAllUsersAsync(serviceProvider);
    }
}