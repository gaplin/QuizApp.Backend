using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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

        services.AddSingleton(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            var client = new MongoClient(settings.ConnectionString);
            return client.GetDatabase(settings.DatabaseName);
        }
        ).AddRepositories();

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddSingleton<IQuizAppContext, QuizAppContext>();
        services.AddTransient<IQuizRepository, QuizRespository>();
        services.AddTransient<IUsersRepository, UsersRepository>();
        return services;
    }
}