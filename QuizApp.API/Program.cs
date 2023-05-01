using QuizApp.Domain;
using QuizApp.Domain.Entities;
using QuizApp.Domain.Interfaces.Services;
using QuizApp.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddCors(options =>
    {
        options.AddDefaultPolicy(
            builder =>
            {
                builder.AllowAnyOrigin();
                builder.AllowAnyMethod();
                builder.AllowAnyHeader();
            });
    })
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddInfrastructure(builder.Configuration.GetSection("QuizAppDatabase"))
    .AddDomain();

var app = builder.Build();

app.UseCors();

if (app.Environment.IsDevelopment())
{
    app
        .UseSwagger()
        .UseSwaggerUI();
}

var quizzes = app.MapGroup("/quizzes");

quizzes.MapGet("/", async (IQuizService service) =>
    await service.GetAsync());

quizzes.MapGet("/baseInfo", async (IQuizService service) =>
    await service.GetBaseInfoAsync());

quizzes.MapGet("/{id:length(24)}", async (string id, bool shuffle, IQuizService service) =>
    await service.GetAsync(id, shuffle)
        is Quiz quiz
            ? Results.Ok(quiz)
            : Results.NotFound());

quizzes.MapPost("/", async (Quiz newQuiz, IQuizService service) =>
{
    await service.InsertAsync(newQuiz);
    return Results.Created($"/quizzes/{newQuiz.Id}", newQuiz);
});

quizzes.MapDelete("/{id:length(24)}", async (string id, IQuizService service) =>
    await service.DeleteAsync(id)
        is true
            ? Results.NoContent()
            : Results.NotFound());

quizzes.MapDelete("/", async (IQuizService service) =>
{
    await service.DeleteAsync();
    return Results.NoContent();
});

app.Run();