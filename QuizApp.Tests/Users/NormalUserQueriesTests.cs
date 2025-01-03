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

public sealed class NormalUserQueriesTests : IClassFixture<QuizApiFixture>, IAsyncLifetime
{
    private User _user = null!; // initialized in InitializeAsync
    private string _token = null!; // initialized in InitializeAsync
    private readonly IServiceProvider _serviceProvider;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

    public NormalUserQueriesTests(QuizApiFixture fixture, ITestOutputHelper outputHelper)
    {
        fixture.OutputHelper = outputHelper;
        _client = fixture.CreateClient();
        _serviceProvider = fixture.Services;
        _serializerOptions.Converters.Add(new JsonStringEnumConverter());
    }

    [Fact]
    public async Task GettingAllUsers_WithAuthHeaderPresent_ReturnsForbidden()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

        // Act
        using var response = await _client.GetAsync("/users", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GettingAllUsers_WithoutAuthHeader_ReturnsUnathorized()
    {
        // Act
        using var response = await _client.GetAsync("/users", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GettingById_DifferentIdThanUser_ReturnsForbidden()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        var param = _user.Id![..^3] + _user.Id[..3];

        // Act
        using var response = await _client.GetAsync($"/users/{param}", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GettingById_WithoutAuthHeader_ReturnsUnathorized()
    {
        // Arrange
        var param = _user.Id;

        // Act
        using var response = await _client.GetAsync($"/users/{param}", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GettingById_SameIdAsUser_ReturnsUserInfo()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        var param = _user.Id;

        // Act
        using var response = await _client.GetAsync($"/users/{param}", TestContext.Current.CancellationToken);

        // Assert
        response.Should().BeSuccessful();
        var userDto = await response.Content.ReadFromJsonAsync<UserDTO>(_serializerOptions, cancellationToken: TestContext.Current.CancellationToken);
        userDto.Should().BeEquivalentTo(new UserDTO { Id = _user.Id!, UserName = _user.UserName, UserType = _user.UserType });
    }

    [Fact]
    public async Task GettingRoleById_WithDifferentUserId_ReturnsForbidden()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        var param = _user.Id![..^3] + _user.Id[..3];

        // Act
        using var response = await _client.GetAsync($"/users/{param}/role", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GettingRoleById_WithoutAuthHeader_ReturnsUnathorized()
    {
        // Arrange
        var param = _user.Id;

        // Act
        using var response = await _client.GetAsync($"/users/{param}/role", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GettingRoleById_CorrectId_ReturnsUserRole()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        var param = _user.Id;

        // Act
        using var response = await _client.GetAsync($"/users/{param}/role", TestContext.Current.CancellationToken);

        // Assert
        response.Should().BeSuccessful();
        var role = await response.Content.ReadFromJsonAsync<EUserType>(_serializerOptions, cancellationToken: TestContext.Current.CancellationToken);
        role.Should().Be(_user.UserType);
    }

    public async ValueTask DisposeAsync()
    {
        await DbUtilities.DeleteAllUsersAsync(_serviceProvider);
    }

    public async ValueTask InitializeAsync()
    {
        _user = await DbUtilities.CreateRandomUserAsync(_serviceProvider, EUserTypeModel.User);

        using var scope = _serviceProvider.CreateAsyncScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        _token = tokenService.GenerateTokenForUser(_user);
    }
}