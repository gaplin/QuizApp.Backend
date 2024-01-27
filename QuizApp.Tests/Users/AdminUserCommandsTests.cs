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

namespace QuizApp.Tests.Users;

public class AdminUserCommandsTests : IClassFixture<QuizApiFixture>, IAsyncLifetime
{
    private User _user = null!; // initialized in InitializeAsync
    private string _token = null!; // initialized in InitializeAsync
    private readonly IServiceProvider _serviceProvider;
    private readonly HttpClient _client;

    public AdminUserCommandsTests(QuizApiFixture fixture, ITestOutputHelper outputHelper)
    {
        fixture.OutputHelper = outputHelper;
        _client = fixture.CreateClient();
        _serviceProvider = fixture.Services;
    }

    [Fact]
    public async Task DeletingAllUsers_WithAuthHeader_ReturnsNoContentAndDeletesAllUsers()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        _ = await CreateRandomUser(EUserTypeModel.Admin);

        // Act
        using var response = await _client.DeleteAsync("/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var allUsers = await GetAllUsersAsync();
        allUsers.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteById_DifferentUser_ReturnsNoContentAndDeletesUser()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        var userToDelete = await CreateRandomUser(EUserTypeModel.User);
        var param = userToDelete.Id;

        // Act
        using var response = await _client.DeleteAsync($"/users/{param}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var allUsers = await GetAllUsersAsync();
        allUsers.Should().NotContain(x => x.Id == userToDelete.Id);
    }

    [Fact]
    public async Task DeleteById_NotExistingUser_ReturnsNotFound()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        var param = _user.Id![..^3] + _user.Id[..3];

        // Act
        using var response = await _client.DeleteAsync($"/users/{param}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
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

    private async Task<List<UserModel>> GetAllUsersAsync()
    {
        using var scope = _serviceProvider.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<IQuizAppContext>();
        var usersCollection = db.Users;

        var users = await usersCollection.Find(_ => true).ToListAsync();

        return users;
    }

    public async Task DisposeAsync()
    {
        using var scope = _serviceProvider.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<IQuizAppContext>();
        MemoryCache cache = (MemoryCache)scope.ServiceProvider.GetRequiredService<IMemoryCache>();
        var users = db.Users;

        await users.DeleteManyAsync(_ => true);
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