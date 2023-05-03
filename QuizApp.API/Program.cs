using Microsoft.AspNetCore.Http.Json;
using QuizApp.Domain;
using QuizApp.Domain.Entities;
using QuizApp.Domain.Interfaces.Services;
using QuizApp.Infrastructure;
using System.Text.Json.Serialization;
using MvcJsonOptions = Microsoft.AspNetCore.Mvc.JsonOptions;

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
    .Configure<JsonOptions>(options =>
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter())
    )
    .Configure<MvcJsonOptions>(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())
    )
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
var users = app.MapGroup("/users");

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

users.MapGet("/", async (IUserService service) =>
    await service.GetAsync());

users.MapGet("/{id:length(24)}", async (string id, IUserService service) =>
    await service.GetAsync(id)
        is User user
            ? Results.Ok(user)
            : Results.NotFound());

users.MapPost("/", async (User newUser, IUserService service) =>
{
    await service.InsertAsync(newUser);
    return Results.Created($"/users/{newUser.Id}", newUser);
});

users.MapDelete("/{id:length(24)}", async (string id, IUserService service) =>
    await service.DeleteAsync(id)
        is true
            ? Results.NoContent()
            : Results.NotFound());

users.MapDelete("/", async (IUserService service) =>
{
    await service.DeleteAsync();
    return Results.NoContent();
});

app.Run();