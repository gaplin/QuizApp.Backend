﻿using Microsoft.Extensions.Caching.Memory;
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

public sealed class NormalUserCommandsTests : IClassFixture<QuizApiFixture>, IAsyncLifetime
{
    private User _user = null!; // initialized in InitializeAsync
    private string _token = null!; // initialized in InitializeAsync
    private readonly IServiceProvider _serviceProvider;
    private readonly HttpClient _client;

    public NormalUserCommandsTests(QuizApiFixture fixture, ITestOutputHelper outputHelper)
    {
        fixture.OutputHelper = outputHelper;
        _client = fixture.CreateClient();
        _serviceProvider = fixture.Services;
    }

    [Fact]
    public async Task DeletingAllUsers_WithAuthHeader_ReturnsForbidden()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

        // Act
        using var response = await _client.DeleteAsync("/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeletingAllUsers_WithoutAuthHeader_ReturnsUnathorized()
    {
        // Act
        using var response = await _client.DeleteAsync("/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteById_WithoutAuthHeader_ReturnsUnathorized()
    {
        // Arrange
        var param = _user.Id;

        // Act
        using var response = await _client.DeleteAsync($"/users/{param}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteById_DifferentUser_ReturnsForbidden()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        var param = _user.Id![..^3] + _user.Id[..3];

        // Act
        using var response = await _client.DeleteAsync($"/users/{param}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteById_SameUser_ReturnsNoContentAndDeletesUser()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        var param = _user.Id;

        // Act
        using var response = await _client.DeleteAsync($"/users/{param}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var allUsers = await GetAllUsersAsync();
        allUsers.Should().NotContain(x => x.Id == _user.Id);
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
        using var scope = _serviceProvider.CreateAsyncScope();

        var userModel = new UserModel
        {
            HPassword = Path.GetRandomFileName(),
            Login = Path.GetRandomFileName(),
            UserName = Path.GetRandomFileName(),
            UserType = EUserTypeModel.User
        };

        var db = scope.ServiceProvider.GetRequiredService<IQuizAppContext>();
        var users = db.Users;

        await users.InsertOneAsync(userModel);
        _user = UserMapper.MapToEntity(userModel);

        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        _token = tokenService.GenerateTokenForUser(_user);
    }
}