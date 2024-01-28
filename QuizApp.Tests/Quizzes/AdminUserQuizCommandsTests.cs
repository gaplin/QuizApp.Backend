using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using QuizApp.Domain.Entities;
using QuizApp.Domain.Interfaces.Services;
using QuizApp.Infrastructure.DbModels;
using QuizApp.Infrastructure.Interfaces;
using QuizApp.Infrastructure.Mappers;
using QuizApp.Tests.Fixtures;
using System.Net;
using System.Net.Http.Headers;
using Xunit.Abstractions;

namespace QuizApp.Tests.Quizzes;

public sealed class AdminUserQuizCommandsTests : IClassFixture<QuizApiFixture>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly IServiceProvider _serviceProvider;
    private string _token = null!; // initialized in InitializeAsync
    private User _user = null!; // initialized in InitializeAsync

    public AdminUserQuizCommandsTests(QuizApiFixture fixture, ITestOutputHelper outputHelper)
    {
        fixture.OutputHelper = outputHelper;
        _serviceProvider = fixture.Services;
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task DeletingAll_ReturnsNoContentAndDeletesAllQuizzes()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        await CreateRandomQuiz("auth1", "auth1");
        await CreateRandomQuiz("auth2", "auth2");

        // Act
        using var response = await _client.DeleteAsync($"/quizzes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var allQuizzes = await GetAllQuizzesAsync();
        allQuizzes.Should().BeEmpty();
    }

    [Fact]
    public async Task DeletingById_QuizCreatedByDifferentUser_ReturnsNoContentAndDeletesQuiz()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        var id = await CreateRandomQuiz("author", "authorId");

        // Act
        using var response = await _client.DeleteAsync($"/quizzes/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var allQuizzes = await GetAllQuizzesAsync();
        allQuizzes.Should().BeEmpty();
    }

    private async Task<User> CreateRandomUser(EUserTypeModel userType)
    {
        var userModel = new UserModel
        {
            HPassword = Path.GetRandomFileName(),
            Login = Path.GetRandomFileName(),
            UserName = Path.GetRandomFileName(),
            UserType = userType
        };
        using var scope = _serviceProvider.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<IQuizAppContext>();
        var users = db.Users;

        await users.InsertOneAsync(userModel);
        var user = UserMapper.MapToEntity(userModel);
        return user;
    }

    private async Task<string> CreateRandomQuiz(string AuthorName, string AuthorId)
    {
        var quizModel = new QuizModel
        {
            Author = AuthorName,
            Category = Path.GetRandomFileName(),
            AuthorId = AuthorId,
            Title = Path.GetRandomFileName(),
            Questions = []
        };
        using var scope = _serviceProvider.CreateAsyncScope();

        var db = _serviceProvider.GetRequiredService<IQuizAppContext>();
        var quizzes = db.Quizzes;

        await quizzes.InsertOneAsync(quizModel);
        return quizModel.Id!;
    }

    private async Task<List<QuizModel>> GetAllQuizzesAsync()
    {
        using var scope = _serviceProvider.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<IQuizAppContext>();
        var quizzes = db.Quizzes;

        var result = await quizzes.Find(_ => true).ToListAsync();
        return result;
    }

    public async Task DisposeAsync()
    {
        using var scope = _serviceProvider.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<IQuizAppContext>();
        MemoryCache cache = (MemoryCache)scope.ServiceProvider.GetRequiredService<IMemoryCache>();
        var users = db.Users;
        var quizzes = db.Quizzes;

        await users.DeleteManyAsync(_ => true);
        await quizzes.DeleteManyAsync(_ => true);
        cache.Clear();
    }

    public async Task InitializeAsync()
    {
        _user = await CreateRandomUser(EUserTypeModel.Admin);
        using var scope = _serviceProvider.CreateAsyncScope();

        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        _token = tokenService.GenerateTokenForUser(_user);
    }
}