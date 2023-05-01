using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace QuizApp.Infrastructure.DbModels;

internal class QuizModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string Title { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string Author { get; set; } = null!;
    public List<QuestionModel> Questions { get; set; } = null!;
}