using Carter;
using QuizApp.Domain.DTOs;
using QuizApp.Domain.Entities;
using QuizApp.Domain.Interfaces.Services;
using System.Security.Claims;

namespace QuizApp.API.Endpoints;

public class QuizzesEndpoints : CarterModule
{
    public QuizzesEndpoints()
        : base("/quizzes")
    {
        WithTags("Quizzes");
    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/", async (IQuizService service) =>
            await service.GetAsync());

        app.MapGet("/baseInfo", async (IQuizService service) =>
            await service.GetBaseInfoAsync());

        app.MapGet("/{id:length(24)}", async (string id, bool shuffle, IQuizService service) =>
            await service.GetAsync(id, shuffle)
                is Quiz quiz
                    ? Results.Ok(quiz)
                    : Results.NotFound());

        app.MapPost("/", async (CreateQuizDTO newQuiz, IQuizService service, ClaimsPrincipal claims) =>
        {
            var validationErrors = await service.InsertAsync(newQuiz, claims);
            if (validationErrors is null)
            {
                return Results.Ok();
            }
            return Results.ValidationProblem(validationErrors);
        }).RequireAuthorization();

        app.MapDelete("/{id:length(24)}", async (string id, IQuizService service, ClaimsPrincipal claims) =>
        {
            var (forbidden, notFound) = await service.DeleteAsync(id, claims);
            if (forbidden) return Results.Forbid();
            if (notFound) return Results.NotFound();
            return Results.NoContent();
        }).RequireAuthorization();

        app.MapDelete("/", async (IQuizService service) =>
        {
            await service.DeleteAsync();
            return Results.NoContent();
        }).RequireAuthorization("Admin");
    }
}