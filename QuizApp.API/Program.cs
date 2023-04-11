using QuizApp.Domain;
using QuizApp.Domain.Entities;
using QuizApp.Domain.Interfaces.Services;
using QuizApp.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();
});
builder.Services.AddInfrastructure(builder.Configuration.GetSection("QuizAppDatabase"));
builder.Services.AddDomain();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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
    await service.UpdateAsync(id, updatedQuiz)
        is true
            ? Results.NoContent()
            : Results.NotFound());

quizzes.MapDelete("/{id:length(24)}", async (string id, IQuizService service) =>
    await service.DeleteAsync(id)
        is true
            ? Results.NoContent()
            : Results.NotFound());

app.Run();