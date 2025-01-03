using Microsoft.Extensions.DependencyInjection;
using QuizApp.Domain.Entities;
using QuizApp.Domain.Interfaces.Services;
using QuizApp.Infrastructure.DbModels;
using QuizApp.Tests.Fixtures;
using QuizApp.Tests.TestsUtils;
using System.Net;
using System.Net.Http.Headers;

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
        await DbUtilities.CreateRandomQuizAsync(_serviceProvider, "author1", "authorId1", 0);
        await DbUtilities.CreateRandomQuizAsync(_serviceProvider, "author2", "authorId2", 1);

        // Act
        using var response = await _client.DeleteAsync($"/quizzes", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var allQuizzes = await DbUtilities.GetAllQuizzesAsync(_serviceProvider);
        allQuizzes.Should().BeEmpty();
    }

    [Fact]
    public async Task DeletingById_QuizCreatedByDifferentUser_ReturnsNoContentAndDeletesQuiz()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        var quiz = await DbUtilities.CreateRandomQuizAsync(_serviceProvider, "author", "authorId", 0);

        // Act
        using var response = await _client.DeleteAsync($"/quizzes/{quiz.Id}", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var allQuizzes = await DbUtilities.GetAllQuizzesAsync(_serviceProvider);
        allQuizzes.Should().BeEmpty();
    }

    public async ValueTask DisposeAsync()
    {
        await DbUtilities.DeleteAllAsync(_serviceProvider);
    }

    public async ValueTask InitializeAsync()
    {
        _user = await DbUtilities.CreateRandomUserAsync(_serviceProvider, EUserTypeModel.Admin);

        using var scope = _serviceProvider.CreateAsyncScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        _token = tokenService.GenerateTokenForUser(_user);
    }
}