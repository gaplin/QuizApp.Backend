using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Swashbuckle.AspNetCore.Annotations;

namespace QuizApp.Domain.Entities;

public class Quiz
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [SwaggerSchema(ReadOnly = true)]
    public string? Id { get; set; }
    public string Title { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string Author { get; set; } = null!;
    public IList<Question> Questions { get; set; } = null!;
}