using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuizApp.Domain.Interfaces.Randomizers;
using QuizApp.Domain.Interfaces.Services;
using QuizApp.Domain.Options;
using QuizApp.Domain.Randomizers;
using QuizApp.Domain.Services;

namespace QuizApp.Domain;

public static class DependencyInjection
{
    public static IServiceCollection AddDomain(this IServiceCollection services, IConfigurationSection configurationSection)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configurationSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddTransient<IQuizService, QuizService>();
        services.AddTransient<IQuizRandomizer, QuizRandomizer>();
        services.AddTransient<IUserService, UserService>();
        services.AddTransient<ILoginService, LoginService>();
        services.AddTransient<ITokenService, TokenService>();
        return services;
    }
}