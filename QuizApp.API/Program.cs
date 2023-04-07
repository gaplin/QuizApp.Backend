using QuizApp.Domain;
using QuizApp.Domain.Entities;
using QuizApp.Domain.Interfaces.Services;
using QuizApp.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
var section = builder.Configuration.GetSection("QuizAppDatabase");
var tf = section["ConnectionString"];

builder.Services.AddInfrastructure(builder.Configuration.GetSection("QuizAppDatabase"));
builder.Services.AddDomain();

var app = builder.Build();

var quizzes = app.MapGroup("/quizzes");

quizzes.MapGet("/", async (IQuizService service) =>
    await service.GetAsync());

quizzes.MapGet("/{id:length(24)}", async (string id, IQuizService service) =>
    await service.GetAsync(id)
        is Quiz quiz
            ? Results.Ok(quiz)
            : Results.NotFound());

quizzes.MapPost("/", async (Quiz newQuiz, IQuizService service) =>
{
    await service.InsertAsync(newQuiz);
    return Results.Created($"/quizzes/{newQuiz.Id}", newQuiz);
});

quizzes.MapPut("/{id:length(24)}", async (string id, Quiz updatedQuiz, IQuizService service) =>
{
    var quiz = await service.GetAsync(id);

    if (quiz is null) return Results.NotFound();

    await service.UpdateAsync(id, updatedQuiz);

    return Results.NoContent();
});

quizzes.MapDelete("/{id:length(24)}", async (string id, IQuizService service) =>
{
    if(await service.GetAsync(id) is not null)
    {
        await service.DeleteAsync(id);
        return Results.NoContent();
    }

    return Results.NotFound();
});

app.MapGet("/", () => "Hello World!");

app.Run();