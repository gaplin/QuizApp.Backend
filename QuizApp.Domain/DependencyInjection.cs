using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuizApp.Domain.Auth;
using QuizApp.Domain.DTOs;
using QuizApp.Domain.Interfaces.Randomizers;
using QuizApp.Domain.Interfaces.Services;
using QuizApp.Domain.Options;
using QuizApp.Domain.Randomizers;
using QuizApp.Domain.Services;
using QuizApp.Domain.Validators;

namespace QuizApp.Domain;

public static class DependencyInjection
{
    public static IServiceCollection AddDomain(this IServiceCollection services, IConfigurationSection configurationSection)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configurationSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddScoped<IClaimsTransformation, AddRoleClaimTransformation>()
            .AddServices()
            .AddValidators();
        return services;
    }
    private static IServiceCollection AddServices(this IServiceCollection services)
        => services
            .AddTransient<IQuizService, QuizService>()
            .AddTransient<IQuizRandomizer, QuizRandomizer>()
            .AddTransient<IUserService, UserService>()
            .AddTransient<ILoginService, LoginService>()
            .AddTransient<ITokenService, TokenService>()
            .AddTransient<IHashService, HashService>();

    private static IServiceCollection AddValidators(this IServiceCollection services)
    {
        ValidatorOptions.Global.LanguageManager.Enabled = false;
        return services
            .AddScoped<IValidator<CredentialsDTO>, CredentialsDTOValidator>()
            .AddScoped<IValidator<CreateUserDTO>, CreateUserDTOValidator>()
            .AddScoped<IValidator<CreateQuestionDTO>, CreateQuestionDTOValidator>()
            .AddScoped<IValidator<CreateQuizDTO>, CreateQuizDTOValidator>();
    }
}