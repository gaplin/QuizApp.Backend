using Microsoft.Extensions.DependencyInjection;
using QuizApp.Domain.Interfaces.Orderers;
using QuizApp.Domain.Interfaces.Services;
using QuizApp.Domain.Orderers;
using QuizApp.Domain.Services;

namespace QuizApp.Domain;

public static class DependencyInjection
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        services.AddTransient<IQuizService, QuizService>();
        services.AddTransient<IQuizReorderer, RandomQuizReorderer>();
        return services;
    }
}