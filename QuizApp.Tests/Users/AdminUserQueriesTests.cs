using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using QuizApp.Domain.DTOs;
using QuizApp.Domain.Entities;
using QuizApp.Domain.Enums;
using QuizApp.Domain.Interfaces.Services;
using QuizApp.Infrastructure.DbModels;
using QuizApp.Infrastructure.Interfaces;
using QuizApp.Infrastructure.Mappers;
using QuizApp.Tests.Fixtures;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit.Abstractions;

namespace QuizApp.Tests.Users;

public class AdminUserQueriesTests : IClassFixture<QuizApiFixture>, IAsyncLifetime
{
    private User _user = null!; // initialized in InitializeAsync
    private string _token = null!; // initialized in InitializeAsync
    private readonly IServiceProvider _serviceProvider;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

    public AdminUserQueriesTests(QuizApiFixture fixture, ITestOutputHelper outputHelper)
    {
        fixture.OutputHelper = outputHelper;
        _client = fixture.CreateClient();
        _serviceProvider = fixture.Services;
        _serializerOptions.Converters.Add(new JsonStringEnumConverter());
    }

    [Fact]
    public async Task GettingAllUsers_ReturnsAllUsers()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        User[] all_users = [_user, await CreateRandomUser(EUserTypeModel.User), await CreateRandomUser(EUserTypeModel.Admin)];
        var expectedResult = all_users.Select(x => new UserDTO { Id = x.Id!, UserName = x.UserName, UserType = x.UserType });

        // Act
        using var response = await _client.GetAsync("/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var users = await response.Content.ReadFromJsonAsync<List<UserDTO>>(_serializerOptions);
        users.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task GettingById_DifferentIdThanUser_DifferentUser()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        var userToQuery = await CreateRandomUser(EUserTypeModel.Admin);
        var param = userToQuery.Id;

        // Act
        using var response = await _client.GetAsync($"/users/{param}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<UserDTO>(_serializerOptions);
        user.Should().BeEquivalentTo(new UserDTO { Id = userToQuery.Id!, UserName = userToQuery.UserName, UserType = userToQuery.UserType });
    }

    [Fact]
    public async Task GettingById_NotExisitngUser_ReturnsNotFound()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        var param = _user.Id![..^3] + _user.Id[..3];

        // Act
        using var response = await _client.GetAsync($"/users/{param}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GettingRoleById_NotExistingUser_ReturnsNotFound()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        var param = _user.Id![..^3] + _user.Id[..3];

        // Act
        using var response = await _client.GetAsync($"/users/{param}/role");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GettingRoleById_WithDifferentUserId_ReturnsDifferentUserRole()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        var userToQuery = await CreateRandomUser(EUserTypeModel.User);
        var param = userToQuery.Id;

        // Act
        using var response = await _client.GetAsync($"/users/{param}/role");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var role = await response.Content.ReadFromJsonAsync<EUserType>(_serializerOptions);
        role.Should().Be(userToQuery.UserType);
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