using Microsoft.IdentityModel.JsonWebTokens;
using QuizApp.Domain.Entities;
using QuizApp.Domain.Enums;
using System.Security.Claims;

namespace QuizApp.API.FIlters;

public class SameIdOrAdminFIlter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var id = context.GetArgument<string>(0);
        var claims = context.HttpContext.User.Claims;
        var claimId = claims.First(x => x.Type == nameof(User.Id)).Value;
        var role = claims.First(x => x.Type == ClaimTypes.Role).Value;
        if (claimId == id || role == EUserType.Admin.ToString())
        {
            return await next(context);
        }

        return Results.Forbid();
    }
}