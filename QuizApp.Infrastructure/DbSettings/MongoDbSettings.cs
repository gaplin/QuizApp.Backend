using System.ComponentModel.DataAnnotations;

namespace QuizApp.Infrastructure.DbSettings;

internal class MongoDbSettings
{
    [Required]
    public string ConnectionString { get; set; } = null!;
    [Required]
    public string DatabaseName { get; set; } = null!;
    [Required]
    public CollectionNamesSettings Collections { get; set; } = null!;
}