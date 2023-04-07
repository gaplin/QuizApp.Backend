using System.ComponentModel.DataAnnotations;

namespace QuizApp.Infrastructure.DbSettings;

internal class MongoDbSettings
{
    [Required]
    public string ConnectionString { get; init; } = null!;
    [Required]
    public string DatabaseName { get; init; } = null!;
    [Required]
    public CollectionNamesSettings Collections { get; init; } = null!;
}