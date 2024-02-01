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
            await service.GetAsync()
            )
            .Produces<List<Quiz>>();

        app.MapGet("/baseInfo", async (IQuizService service) =>
            await service.GetBaseInfoAsync()
            )
            .Produces<List<QuizBase>>();

        app.MapGet("/{id:length(24)}", async (string id, bool shuffle, IQuizService service) =>
            await service.GetAsync(id, shuffle)
                is Quiz quiz
                    ? Results.Ok(quiz)
                    : Results.NotFound())
            .Produces<Quiz>()
            .Produces(StatusCodes.Status404NotFound);

        app.MapPost("/", async (CreateQuizDTO newQuiz, IQuizService service, ClaimsPrincipal claims) =>
        {
            var validationErrors = await service.InsertAsync(newQuiz, claims);
            if (validationErrors is null)
            {
                return Results.Ok();
            }
            return Results.ValidationProblem(validationErrors);
        })
            .RequireAuthorization()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem();

        app.MapDelete("/{id:length(24)}", async (string id, IQuizService service, ClaimsPrincipal claims) =>
        {
            var (forbidden, notFound) = await service.DeleteAsync(id, claims);
            if (forbidden) return Results.Forbid();
            if (notFound) return Results.NotFound();
            return Results.NoContent();
        })
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        app.MapDelete("/", async (IQuizService service) =>
        {
            await service.DeleteAsync();
            return Results.NoContent();
        })
            .RequireAuthorization("Admin")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
    }
}