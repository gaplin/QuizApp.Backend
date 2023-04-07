using System.ComponentModel.DataAnnotations;

namespace QuizApp.Infrastructure.DbSettings;

internal class MongoDbSettings
{
    [Required]
    public required string ConnectionString { get; init; } = null!;
    [Required]
    public required string DatabaseName { get; init; } = null!;
    [Required]
    public required CollectionNamesSettings Collections { get; init; } = null!;
}