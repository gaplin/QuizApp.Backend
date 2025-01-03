using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using QuizApp.Domain.DTOs;
using QuizApp.Domain.Entities;
using QuizApp.Domain.Interfaces.Services;
using QuizApp.Infrastructure.DbModels;
using QuizApp.Tests.Fixtures;
using QuizApp.Tests.TestsUtils;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace QuizApp.Tests.Quizzes;

public sealed class NormalUserQuizCommandsTests : IClassFixture<QuizApiFixture>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly IServiceProvider _serviceProvider;
    private string _token = null!; // initialized in InitializeAsync
    private User _user = null!; // initialized in InitializeAsync

    public NormalUserQuizCommandsTests(QuizApiFixture fixture, ITestOutputHelper outputHelper)
    {
        fixture.OutputHelper = outputHelper;
        _serviceProvider = fixture.Services;
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task Creating_WithNotLoggedUser_ReturnsUnathorized()
    {
        // Arrange
        var quiz = new CreateQuizDTO();

        // Act
        using var response = await _client.PostAsJsonAsync("/quizzes", quiz, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Creating_WithValidData_ReturnsOkAndInsertsQuiz()
    {
        // Arrange
        var quiz = new CreateQuizDTO
        {
            Category = "Math",
            Title = "Numbers",
            Questions =
            [
                new CreateQuestionDTO
                {
                    Text = "What's the result of 2 + 2?",
                    Answers =
                    [
                        "3",
                        "4",
                        "2"
                    ],
                    CorrectAnswer = 1
                },
                new CreateQuestionDTO
                {
                    Text = "Which number is prime?",
                    Answers =
                    [
                        "2",
                        "1",
                        "9",
                        "27"
                    ],
                    CorrectAnswer = 0
                }
            ]
        };
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

        // Act
        using var response = await _client.PostAsJsonAsync("/quizzes", quiz, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var allQuizzes = await DbUtilities.GetAllQuizzesAsync(_serviceProvider);
        allQuizzes.Should().HaveCount(1);
        allQuizzes[0].Should().BeEquivalentTo(quiz);
    }

    [Theory]
    [InlineData(-1, 3)]
    [InlineData(3, 3)]
    [InlineData(4, 3)]
    public async Task Creating_WithInvalidCorrectAnswerNumber_ReturnsBadRequest(int answerNum, int answersCount)
    {
        // Arrange
        var quiz = new CreateQuizDTO
        {
            Category = "Math",
            Title = "Numbers",
            Questions =
            [
                new CreateQuestionDTO
                {
                    Text = "What's the result of 2 + 2?",
                    Answers = Enumerable.Range(0, answersCount).Select(x => x.ToString()).ToList(),
                    CorrectAnswer = answerNum
                }
            ]
        };
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

        // Act
        using var response = await _client.PostAsJsonAsync("/quizzes", quiz, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problemDetails = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(cancellationToken: TestContext.Current.CancellationToken);
        problemDetails!.Errors["Questions[0].CorrectAnswer"].Should().ContainSingle($"Value must be between 0 and {answersCount - 1} inclusive");
    }

    [Fact]
    public async Task DeletingAll_WithoutAuthHeader_ReturnsUnathorized()
    {
        // Act
        using var response = await _client.DeleteAsync($"/quizzes", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeletingAll_WithAuthHeader_ReturnsForbidden()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

        // Act
        using var response = await _client.DeleteAsync($"/quizzes", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeletingById_WithoutAuthHeader_ReturnsUnathorized()
    {
        // Arrange
        var id = new string('a', 24);

        // Act
        using var response = await _client.DeleteAsync($"/quizzes/{id}", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeletingById_NotExistingQuiz_ReturnsNotFound()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        var id = new string('a', 24);

        // Act
        using var response = await _client.DeleteAsync($"/quizzes/{id}", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeletingById_QuizCreatedByDifferentUser_ReturnsForbidden()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        var quiz = await DbUtilities.CreateRandomQuizAsync(_serviceProvider, "author", "authorId", 0);

        // Act
        using var response = await _client.DeleteAsync($"/quizzes/{quiz.Id}", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeletingById_QuizCreatedByGivenUser_ReturnsNoContentAndDeletesQuiz()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        var quiz = await DbUtilities.CreateRandomQuizAsync(_serviceProvider, _user.UserName, _user.Id!, 1);

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
        _user = await DbUtilities.CreateRandomUserAsync(_serviceProvider, EUserTypeModel.User);

        using var scope = _serviceProvider.CreateAsyncScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        _token = tokenService.GenerateTokenForUser(_user);
    }
}