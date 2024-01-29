using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using QuizApp.Domain.Entities;
using QuizApp.Tests.Fixtures;
using QuizApp.Tests.TestsUtils;
using System.Net;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace QuizApp.Tests.Quizzes;

public sealed class QuizzesAnonymousQueriesTests : IClassFixture<QuizApiFixture>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly IServiceProvider _serviceProvider;

    public QuizzesAnonymousQueriesTests(QuizApiFixture fixture, ITestOutputHelper outputHelper)
    {
        fixture.OutputHelper = outputHelper;
        _serviceProvider = fixture.Services;
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task GettingAllQuizzes_ReturnsAllQuizzes()
    {
        // Arrange
        var allQuizzes = await DbUtilities.CreateRandomQuizzesAsync(_serviceProvider, 3);

        // Act
        using var response = await _client.GetAsync("/quizzes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var resultList = await response.Content.ReadFromJsonAsync<List<Quiz>>();
        resultList.Should().BeEquivalentTo(allQuizzes);
    }

    [Fact]
    public async Task GettingBaseInfo_ReturnsBaseInfoForAllQuizzes()
    {
        // Arrrange
        var allQuizzes = await DbUtilities.CreateRandomQuizzesAsync(_serviceProvider, 3);
        var baseInfos = allQuizzes.Select(x => new QuizBase
        {
            Id = x.Id,
            Author = x.Author,
            AuthorId = x.AuthorId,
            Category = x.Category,
            NumberOfQuestions = x.Questions.Count,
            Title = x.Title,
        });

        // Act
        using var response = await _client.GetAsync("/quizzes/baseInfo");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var resultList = await response.Content.ReadFromJsonAsync<List<QuizBase>>();
        resultList.Should().BeEquivalentTo(baseInfos);
    }

    [Fact]
    public async Task GettingById_WithoutShuffleQueryParam_ReturnsBadRequest()
    {
        // Arrange
        var id = new string('a', 24);

        // Act
        using var response = await _client.GetAsync($"/quizzes/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GettingById_NotExistingQuiz_ReturnsNotFound()
    {
        // Arrange
        var id = new string('a', 24);

        // Act
        using var response = await _client.GetAsync($"/quizzes/{id}?shuffle=false");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GettingById_WithoutShuffle_ReturnsQuizWithQuestionsInTheSameOrder()
    {
        // Arrange
        var quiz = await DbUtilities.CreateRandomQuizAsync(_serviceProvider, "auth", "authId", 10);

        // Act
        using var response = await _client.GetAsync($"/quizzes/{quiz.Id}?shuffle=false");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var quizResult = await response.Content.ReadFromJsonAsync<Quiz>();
        quizResult.Should().BeEquivalentTo(quiz, opts => opts.WithStrictOrdering());
    }

    [Fact]
    public async Task GettingById_WithShuffle_ReturnsQuizWithShuffledQuestionsAndAnswers()
    {
        // Arrange
        var quiz = await DbUtilities.CreateRandomQuizAsync(_serviceProvider, "auth", "authId", 10);

        // Act
        using var response = await _client.GetAsync($"/quizzes/{quiz.Id}?shuffle=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var quizResult = await response.Content.ReadFromJsonAsync<Quiz>();
        quizResult.Should().NotBeEquivalentTo(quiz, opts => opts.WithStrictOrdering());
        quizResult.Should().BeEquivalentTo(quiz, opts => opts.Excluding(x => x.Questions));
        quizResult!.Questions.Should().AllSatisfy(question =>
        {
            var sourceQuestion = quiz.Questions.Single(x => x.Text == question.Text);
            question.Answers.Should().BeEquivalentTo(sourceQuestion.Answers);
            question.Answers[question.CorrectAnswer].Should().Be(sourceQuestion.Answers[sourceQuestion.CorrectAnswer]);
        });
    }

    public async Task DisposeAsync()
    {
        await DbUtilities.DeleteAllQuizzesAsync(_serviceProvider);
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }
}