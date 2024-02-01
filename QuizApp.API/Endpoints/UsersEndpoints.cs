using Carter;
using QuizApp.API.FIlters;
using QuizApp.Domain.DTOs;
using QuizApp.Domain.Enums;
using QuizApp.Domain.Interfaces.Services;

namespace QuizApp.API.Endpoints;

public class UsersEndpoints : CarterModule
{
    public UsersEndpoints()
        : base("/users")
    {
        WithTags("Users");
    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/", async (IUserService service) =>
            await service.GetAsync()
            )
            .RequireAuthorization("Admin")
            .Produces<UserDTO>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        app.MapGet("/{id:length(24)}", async (string id, IUserService service) =>
            await service.GetByIdAsync(id)
                is UserDTO user
                    ? Results.Ok(user)
                    : Results.NotFound())
            .RequireAuthorization()
            .AddEndpointFilter<SameIdOrAdminFIlter>()
            .Produces<UserDTO>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        app.MapGet("/{id:length(24)}/role", async (string id, IUserService service) =>
            await service.GetUserRoleAsync(id)
                is EUserType role
                    ? Results.Ok(role)
                    : Results.NotFound())
            .RequireAuthorization()
            .AddEndpointFilter<SameIdOrAdminFIlter>()
            .Produces<EUserType>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        app.MapPost("/", async (CreateUserDTO newUser, IUserService service) =>
        {
            var (token, errors) = await service.CreateAsync(newUser);
            if (errors is null)
            {
                return Results.Ok(token);
            }
            return Results.ValidationProblem(errors);
        })
            .Produces<string>()
            .ProducesValidationProblem();

        app.MapDelete("/{id:length(24)}", async (string id, IUserService service) =>
            await service.DeleteAsync(id)
                is true
                    ? Results.NoContent()
                    : Results.NotFound())
            .RequireAuthorization()
            .AddEndpointFilter<SameIdOrAdminFIlter>()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        app.MapDelete("/", async (IUserService service) =>
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