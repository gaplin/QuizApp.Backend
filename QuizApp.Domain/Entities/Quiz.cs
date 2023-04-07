using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace QuizApp.Domain.Entities;

public class Quiz
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string Title { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string Author { get; set; } = null!;
    public IList<Question> Questions { get; set; } = null!;
}