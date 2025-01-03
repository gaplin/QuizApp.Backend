using Microsoft.Extensions.DependencyInjection;
using QuizApp.Domain.DTOs;
using QuizApp.Domain.Entities;
using QuizApp.Domain.Enums;
using QuizApp.Domain.Interfaces.Services;
using QuizApp.Infrastructure.DbModels;
using QuizApp.Tests.Fixtures;
using QuizApp.Tests.TestsUtils;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuizApp.Tests.Users;

public sealed class AdminUserQueriesTests : IClassFixture<QuizApiFixture>, IAsyncLifetime
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
        User[] all_users =
            [
                _user,
                await DbUtilities.CreateRandomUserAsync(_serviceProvider, EUserTypeModel.User),
                await DbUtilities.CreateRandomUserAsync(_serviceProvider, EUserTypeModel.Admin)
            ];
        var expectedResult = all_users.Select(x => new UserDTO { Id = x.Id!, UserName = x.UserName, UserType = x.UserType });

        // Act
        using var response = await _client.GetAsync("/users", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var users = await response.Content.ReadFromJsonAsync<List<UserDTO>>(_serializerOptions, cancellationToken: TestContext.Current.CancellationToken);
        users.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task GettingById_DifferentIdThanUser_DifferentUser()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        var userToQuery = await DbUtilities.CreateRandomUserAsync(_serviceProvider, EUserTypeModel.Admin);
        var param = userToQuery.Id;

        // Act
        using var response = await _client.GetAsync($"/users/{param}", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<UserDTO>(_serializerOptions, cancellationToken: TestContext.Current.CancellationToken);
        user.Should().BeEquivalentTo(new UserDTO { Id = userToQuery.Id!, UserName = userToQuery.UserName, UserType = userToQuery.UserType });
    }

    [Fact]
    public async Task GettingById_NotExisitngUser_ReturnsNotFound()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        var param = _user.Id![..^3] + _user.Id[..3];

        // Act
        using var response = await _client.GetAsync($"/users/{param}", TestContext.Current.CancellationToken);

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
        using var response = await _client.GetAsync($"/users/{param}/role", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GettingRoleById_WithDifferentUserId_ReturnsDifferentUserRole()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        var userToQuery = await DbUtilities.CreateRandomUserAsync(_serviceProvider, EUserTypeModel.User);
        var param = userToQuery.Id;

        // Act
        using var response = await _client.GetAsync($"/users/{param}/role", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var role = await response.Content.ReadFromJsonAsync<EUserType>(_serializerOptions, cancellationToken: TestContext.Current.CancellationToken);
        role.Should().Be(userToQuery.UserType);
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