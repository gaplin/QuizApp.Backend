using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using QuizApp.Domain.Interfaces.Repositories;
using QuizApp.Infrastructure.Context;
using QuizApp.Infrastructure.DbSettings;
using QuizApp.Infrastructure.Interfaces;
using QuizApp.Infrastructure.Repositories;

namespace QuizApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfigurationSection configurationSection)
    {
        services.AddOptions<MongoDbSettings>()
            .Bind(configurationSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        var settings = configurationSection.Get<MongoDbSettings>();

        services.AddMongoDatabase(settings!);
        services.AddSingleton<IQuizAppContext, QuizAppContext>();
        services.AddTransient<IQuizRepository, QuizRespository>();
        services.AddTransient<IUsersRepository, UsersRepository>();

        return services;
    }

    private static void AddMongoDatabase(this IServiceCollection services, MongoDbSettings settings)
    {
        services.AddSingleton(sp =>
        {
            var client = new MongoClient(settings.ConnectionString);
            return client.GetDatabase(settings.DatabaseName);
        });
    }
}