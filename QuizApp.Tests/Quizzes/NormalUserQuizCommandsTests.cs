using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using QuizApp.Domain.DTOs;
using QuizApp.Domain.Entities;
using QuizApp.Domain.Interfaces.Services;
using QuizApp.Infrastructure.DbModels;
using QuizApp.Infrastructure.Interfaces;
using QuizApp.Infrastructure.Mappers;
using QuizApp.Tests.Fixtures;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace QuizApp.Tests.Quizzes;

public class NormalUserQuizCommandsTests : IClassFixture<QuizApiFixture>, IAsyncLifetime
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
        using var response = await _client.PostAsJsonAsync("/quizzes", quiz);

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
        using var response = await _client.PostAsJsonAsync("/quizzes", quiz);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var allQuizzes = await GetAllQuizzesAsync();
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
        using var response = await _client.PostAsJsonAsync("/quizzes", quiz);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problemDetails = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
        problemDetails!.Errors["Questions[0].CorrectAnswer"].Should().ContainSingle($"Value must be between 0 and {answersCount - 1} inclusive");
    }

    [Fact]
    public async Task DeletingAll_WithoutAuthHeader_ReturnsUnathorized()
    {
        // Act
        using var response = await _client.DeleteAsync($"/quizzes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeletingAll_WithAuthHeader_ReturnsForbidden()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

        // Act
        using var response = await _client.DeleteAsync($"/quizzes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeletingById_WithoutAuthHeader_ReturnsUnathorized()
    {
        // Arrange
        var id = new string('a', 24);

        // Act
        using var response = await _client.DeleteAsync($"/quizzes/{id}");

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
        using var response = await _client.DeleteAsync($"/quizzes/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeletingById_QuizCreatedByDifferentUser_ReturnsForbidden()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        var id = await CreateRandomQuiz("author", "authorId");

        // Act
        using var response = await _client.DeleteAsync($"/quizzes/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeletingById_QuizCreatedByGivenUser_ReturnsNoContentAndDeletesQuiz()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        var id = await CreateRandomQuiz(_user.UserName, _user.Id!);

        // Act
        using var response = await _client.DeleteAsync($"/quizzes/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var allQuizzes = await GetAllQuizzesAsync();
        allQuizzes.Should().BeEmpty();
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

    private async Task<string> CreateRandomQuiz(string AuthorName, string AuthorId)
    {
        var quizModel = new QuizModel
        {
            Author = AuthorName,
            Category = Path.GetRandomFileName(),
            AuthorId = AuthorId,
            Title = Path.GetRandomFileName(),
            Questions = []
        };
        using var scope = _serviceProvider.CreateAsyncScope();

        var db = _serviceProvider.GetRequiredService<IQuizAppContext>();
        var quizzes = db.Quizzes;

        await quizzes.InsertOneAsync(quizModel);
        return quizModel.Id!;
    }

    private async Task<List<QuizModel>> GetAllQuizzesAsync()
    {
        using var scope = _serviceProvider.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<IQuizAppContext>();
        var quizzes = db.Quizzes;

        var result = await quizzes.Find(_ => true).ToListAsync();
        return result;
    }

    public async Task DisposeAsync()
    {
        using var scope = _serviceProvider.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<IQuizAppContext>();
        MemoryCache cache = (MemoryCache)scope.ServiceProvider.GetRequiredService<IMemoryCache>();
        var users = db.Users;
        var quizzes = db.Quizzes;

        await users.DeleteManyAsync(_ => true);
        await quizzes.DeleteManyAsync(_ => true);
        cache.Clear();
    }

    public async Task InitializeAsync()
    {
        _user = await CreateRandomUser(EUserTypeModel.User);
        using var scope = _serviceProvider.CreateAsyncScope();

        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        _token = tokenService.GenerateTokenForUser(_user);
    }
}