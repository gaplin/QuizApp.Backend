using Microsoft.Extensions.DependencyInjection;
using QuizApp.Domain.Interfaces.Services;
using QuizApp.Domain.Services;

namespace QuizApp.Domain;

public static class DependencyInjection
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        services.AddTransient<IQuizService, QuizService>();
        return services;
    }
}