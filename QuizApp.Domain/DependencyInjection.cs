using Microsoft.Extensions.DependencyInjection;
using QuizApp.Domain.Interfaces.Randomizers;
using QuizApp.Domain.Interfaces.Services;
using QuizApp.Domain.Randomizers;
using QuizApp.Domain.Services;

namespace QuizApp.Domain;

public static class DependencyInjection
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        services.AddTransient<IQuizService, QuizService>();
        services.AddTransient<IQuizRandomizer, QuizRandomizer>();
        services.AddTransient<IUserService, UserService>();
        return services;
    }
}