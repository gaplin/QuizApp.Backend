using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using QuizApp.Domain.DTOs;
using QuizApp.Infrastructure.Interfaces;
using QuizApp.Tests.Fixtures;
using System.Net.Http.Json;
using Xunit.Abstractions;
using MongoDB.Driver;
using QuizApp.Infrastructure.DbModels;
using QuizApp.Infrastructure.Mappers;
using System.Net;
using Microsoft.AspNetCore.Http;

namespace QuizApp.Tests.Users;

public sealed class UserLoginAndCreationTests : IClassFixture<QuizApiFixture>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly IServiceProvider _serviceProvider;

    public UserLoginAndCreationTests(QuizApiFixture fixture, ITestOutputHelper outputHelper)
    {
        fixture.OutputHelper = outputHelper;
        _serviceProvider = fixture.Services;
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task CreationWithTooShortLogin_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateUserDTO
        {
            Login = "l",
            Password = Path.GetRandomFileName(),
            UserName = Path.GetRandomFileName()
        };

        // Act
        using var response = await _client.PostAsJsonAsync("/users", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problemDetails = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
        problemDetails!.Errors["Login"].Should().ContainMatch(("*at least 5 characters*"));
    }

    [Fact]
    public async Task User_GetsToken_AfterCreation()
    {
        // Arrange
        var createDto = new CreateUserDTO
        {
            Login = Path.GetRandomFileName(),
            Password = Path.GetRandomFileName(),
            UserName = Path.GetRandomFileName()
        };

        // Act
        using var response = await _client.PostAsJsonAsync("/users", createDto);

        // Assert
        response.Should().BeSuccessful();
        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task User_CanLogin_AfterCreation()
    {
        // Arrange
        var createDto = new CreateUserDTO
        {
            Login = Path.GetRandomFileName(),
            Password = Path.GetRandomFileName(),
            UserName = Path.GetRandomFileName()
        };
        using var createResponse = await _client.PostAsJsonAsync("/users", createDto);
        createResponse.EnsureSuccessStatusCode();
        var credentials = new CredentialsDTO
        {
            Login = createDto.Login,
            Password = createDto.Password,
        };

        // Act
        using var response = await _client.PostAsJsonAsync("/login", credentials);

        // Assert
        response.Should().BeSuccessful();
        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task LoginAfterCreation_WithInvalidPassword_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateUserDTO
        {
            Login = Path.GetRandomFileName(),
            Password = Path.GetRandomFileName(),
            UserName = Path.GetRandomFileName()
        };
        using var createResponse = await _client.PostAsJsonAsync("/users", createDto);
        createResponse.EnsureSuccessStatusCode();
        var credentials = new CredentialsDTO
        {
            Login = createDto.Login,
            Password = createDto.Password + 'a',
        };

        // Act
        using var response = await _client.PostAsJsonAsync("/login", credentials);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problemDetails = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
        problemDetails!.Errors["Login,Password"].Should().Contain("Invalid Login or Password");
    }

    [Fact]
    public async Task Login_WithNonExistingUser_ReturnsBadRequest()
    {
        var credentials = new CredentialsDTO
        {
            Login = "Loginnn",
            Password = "PPassword",
        };

        // Act
        using var response = await _client.PostAsJsonAsync("/login", credentials);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problemDetails = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
        problemDetails!.Errors["Login,Password"].Should().Contain("Invalid Login or Password");
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
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
}