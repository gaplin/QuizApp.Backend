using QuizApp.Domain.DTOs;
using QuizApp.Tests.Fixtures;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace QuizApp.Tests.Users;

public sealed class UserCreationTests : IClassFixture<QuizApiFixture>, IDisposable
{
    private readonly HttpClient _client;

    public UserCreationTests(QuizApiFixture fixture, ITestOutputHelper outputHelper)
    {
        fixture.OutputHelper = outputHelper;
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task User_GetsToken_AfterCreation()
    {
        // Arrange
        var credentials = new CreateUserDTO
        {
            Login = "lkjljlhk",
            Password = "lkjljlhk",
            UserName = "lkjljlhk"
        };

        // Act
        using var response = await _client.PostAsJsonAsync("/users", credentials);

        // Assert
        response.Should().BeSuccessful();
        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotBeNullOrWhiteSpace();
    }

    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }
}