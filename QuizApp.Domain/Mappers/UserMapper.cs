using QuizApp.Domain.DTOs;
using QuizApp.Domain.Entities;

namespace QuizApp.Domain.Mappers;

internal static class UserMapper
{
    internal static UserDTO MapEntityToDto(User entity)
        => new()
        {
            Id = entity.Id!,
            UserName = entity.UserName,
            UserType = entity.UserType
        };
}