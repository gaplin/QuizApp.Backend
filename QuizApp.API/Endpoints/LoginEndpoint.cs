using Carter;
using QuizApp.Domain.DTOs;
using QuizApp.Domain.Interfaces.Services;

namespace QuizApp.API.Endpoints;

public class LoginEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/login", async (CredentialsDTO credentials, ILoginService service) =>
        {
            var (token, errors) = await service.LogInAndGetTokenAsync(credentials);
            if (errors is null) return Results.Ok(token!);
            return Results.ValidationProblem(errors);
        }
        )
            .WithTags("Login")
            .Produces<string>()
            .ProducesValidationProblem();
    }
}