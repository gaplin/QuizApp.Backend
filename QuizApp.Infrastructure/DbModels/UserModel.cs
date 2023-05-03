using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace QuizApp.Infrastructure.DbModels;

internal class UserModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string UserName { get; set; } = null!;
    public string Login { get; set; } = null!;
    public string HPassword { get; set; } = null!;

    [BsonRepresentation(BsonType.String)]
    public EUserTypeModel UserType { get; set; }
}