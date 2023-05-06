using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.JsonWebTokens;
using QuizApp.Domain.Entities;
using QuizApp.Domain.Interfaces.Repositories;
using System.Security.Claims;

namespace QuizApp.Domain.Auth;

internal class AddRoleClaimTransformation : IClaimsTransformation
{
    private readonly IUsersRepository _usersRepository;

    public AddRoleClaimTransformation(IUsersRepository usersRepository)
    {
        _usersRepository = usersRepository;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var id = principal.Claims.FirstOrDefault(c => c.Type == nameof(User.Id));
        if (id is null) return principal;

        var user = await _usersRepository.GetByIdAsync(id.Value);
        if (user is null) return principal;

        var clone = principal.Clone();
        var newIdentity = (ClaimsIdentity)clone.Identity!;
        var roleClaim = new Claim(ClaimTypes.Role, user.UserType.ToString());

        newIdentity.AddClaim(roleClaim);

        return clone;
    }
}