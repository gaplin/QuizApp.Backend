using QuizApp.Domain.Entities;
using QuizApp.Domain.Enums;
using QuizApp.Infrastructure.DbModels;

namespace QuizApp.Infrastructure.Mappers;

internal static class UserMapper
{
    internal static UserModel MapToModel(User entity) => new()
    {
        Id = entity.Id,
        UserName = entity.UserName,
        Login = entity.Login,
        HPassword = entity.HPassword,
        UserType = Enum.Parse<EUserTypeModel>(entity.UserType.ToString())
    };

    internal static User MapToEntity(UserModel model) => new()
    {
        Id = model.Id,
        UserName = model.UserName,
        Login = model.Login,
        HPassword = model.HPassword,
        UserType = Enum.Parse<EUserType>(model.UserType.ToString())
    };
}