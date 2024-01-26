using QuizApp.Domain.DTOs;
using QuizApp.Tests.Fixtures;
using System.Net;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace QuizApp.Tests.Quizzes;

public sealed class QuizCreationTests : IClassFixture<QuizApiFixture>, IDisposable
{
    private readonly HttpClient _client;

    public QuizCreationTests(QuizApiFixture fixture, ITestOutputHelper outputHelper)
    {
        fixture.OutputHelper = outputHelper;
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task Creating_WithNotLoggedUser_ReturnsUnathorized()
    {
        // Arrange
        var quiz = new CreateQuizDTO();

        // Act
        using var response = await _client.PostAsJsonAsync("/quizzes", quiz);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }
}