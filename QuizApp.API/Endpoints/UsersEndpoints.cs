using Carter;
using QuizApp.API.FIlters;
using QuizApp.Domain.DTOs;
using QuizApp.Domain.Enums;
using QuizApp.Domain.Interfaces.Services;

namespace QuizApp.API.Endpoints;

public class UsersEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/users").WithTags("Users");

        group.MapGet("/", async (IUserService service) =>
            await service.GetAsync()
            )
            .RequireAuthorization("Admin")
            .Produces<UserDTO>()
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/{id:length(24)}", async (string id, IUserService service) =>
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

        group.MapGet("/{id:length(24)}/role", async (string id, IUserService service) =>
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

        group.MapPost("/", async (CreateUserDTO newUser, IUserService service) =>
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

        group.MapDelete("/{id:length(24)}", async (string id, IUserService service) =>
            await service.DeleteAsync(id)
                    ? Results.NoContent()
                    : Results.NotFound())
            .RequireAuthorization()
            .AddEndpointFilter<SameIdOrAdminFIlter>()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/", async (IUserService service) =>
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