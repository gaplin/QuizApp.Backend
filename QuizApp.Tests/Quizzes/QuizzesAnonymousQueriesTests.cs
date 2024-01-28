using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using QuizApp.Domain.Entities;
using QuizApp.Infrastructure.DbModels;
using QuizApp.Infrastructure.Interfaces;
using QuizApp.Tests.Fixtures;
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
        var allQuizzes = await CreateRandomQuizzesAsync(3);

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
        var allQuizzes = await CreateRandomQuizzesAsync(3);
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
        var quiz = await CreateRandomQuizAsync("auth", "authId", 10);

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
        var quiz = await CreateRandomQuizAsync("auth", "authId", 10);

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

    private async Task<List<QuizModel>> CreateRandomQuizzesAsync(int count)
    {
        var result = new List<QuizModel>();
        while (count-- > 0)
        {
            var quiz = await CreateRandomQuizAsync(Path.GetRandomFileName(), Path.GetRandomFileName(), Random.Shared.Next(1, 5));
            result.Add(quiz);
        }
        return result;
    }

    private async Task<QuizModel> CreateRandomQuizAsync(string AuthorName, string AuthorId, int numOfQuestions)
    {
        var quizModel = new QuizModel
        {
            Author = AuthorName,
            Category = Path.GetRandomFileName(),
            AuthorId = AuthorId,
            Title = Path.GetRandomFileName(),
            Questions = Enumerable.Range(0, numOfQuestions).Select(_ => CreateQuestion(Random.Shared.Next(1, 5))).ToList()
        };
        using var scope = _serviceProvider.CreateAsyncScope();

        var db = _serviceProvider.GetRequiredService<IQuizAppContext>();
        var quizzes = db.Quizzes;

        await quizzes.InsertOneAsync(quizModel);
        return quizModel;
    }

    private static QuestionModel CreateQuestion(int numOfAnswers)
    {
        var question = new QuestionModel
        {
            Text = Path.GetRandomFileName(),
            CorrectAnswer = Random.Shared.Next(0, numOfAnswers - 1),
            Answers = Enumerable.Range(0, numOfAnswers).Select(_ => Path.GetRandomFileName()).ToList()
        };
        return question;
    }

    public async Task DisposeAsync()
    {
        using var scope = _serviceProvider.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<IQuizAppContext>();
        var quizzes = db.Quizzes;

        await quizzes.DeleteManyAsync(_ => true);
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }
}