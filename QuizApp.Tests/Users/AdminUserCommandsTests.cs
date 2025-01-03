using Microsoft.Extensions.DependencyInjection;
using QuizApp.Domain.Entities;
using QuizApp.Domain.Interfaces.Services;
using QuizApp.Infrastructure.DbModels;
using QuizApp.Tests.Fixtures;
using QuizApp.Tests.TestsUtils;
using System.Net;
using System.Net.Http.Headers;

namespace QuizApp.Tests.Users;

public sealed class AdminUserCommandsTests : IClassFixture<QuizApiFixture>, IAsyncLifetime
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
        _ = await DbUtilities.CreateRandomUserAsync(_serviceProvider, EUserTypeModel.Admin);

        // Act
        using var response = await _client.DeleteAsync("/users", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var allUsers = await DbUtilities.GetAllUsersAsync(_serviceProvider);
        allUsers.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteById_DifferentUser_ReturnsNoContentAndDeletesUser()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        var userToDelete = await DbUtilities.CreateRandomUserAsync(_serviceProvider, EUserTypeModel.User);
        var param = userToDelete.Id;

        // Act
        using var response = await _client.DeleteAsync($"/users/{param}", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var allUsers = await DbUtilities.GetAllUsersAsync(_serviceProvider);
        allUsers.Should().NotContain(x => x.Id == userToDelete.Id);
    }

    [Fact]
    public async Task DeleteById_NotExistingUser_ReturnsNotFound()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        var param = _user.Id![..^3] + _user.Id[..3];

        // Act
        using var response = await _client.DeleteAsync($"/users/{param}", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    public async ValueTask DisposeAsync()
    {
        await DbUtilities.DeleteAllUsersAsync(_serviceProvider);
    }

    public async ValueTask InitializeAsync()
    {
        _user = await DbUtilities.CreateRandomUserAsync(_serviceProvider, EUserTypeModel.Admin);

        using var scope = _serviceProvider.CreateAsyncScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        _token = tokenService.GenerateTokenForUser(_user);
    }
}