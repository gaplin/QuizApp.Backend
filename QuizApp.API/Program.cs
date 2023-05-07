using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuizApp.API.FIlters;
using QuizApp.Domain;
using QuizApp.Domain.DTOs;
using QuizApp.Domain.Entities;
using QuizApp.Domain.Enums;
using QuizApp.Domain.Interfaces.Services;
using QuizApp.Infrastructure;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using MvcJsonOptions = Microsoft.AspNetCore.Mvc.JsonOptions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddCors(options =>
    {
        options.AddDefaultPolicy(
            builder =>
            {
                builder.AllowAnyOrigin();
                builder.AllowAnyMethod();
                builder.AllowAnyHeader();
            });
    })
    .Configure<JsonOptions>(options =>
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter())
    )
    .Configure<MvcJsonOptions>(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())
    )
    .AddEndpointsApiExplorer()
    .AddSwaggerGen(c =>
    {
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = """
                      JWT Authorization header using the Bearer scheme.<br>
                      Enter 'Bearer' [space] and then your token in the text input below.<br>
                      Example: 'Bearer 12345abcdef'
                      """,
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    })
    .AddMemoryCache()
    .AddInfrastructure(builder.Configuration.GetSection("QuizAppDatabase"))
    .AddDomain(builder.Configuration.GetSection("Jwt"));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ClockSkew = TimeSpan.Zero,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Admin", policy =>
        policy.RequireRole("Admin"))
    .AddDefaultPolicy("RolePresent", policy =>
        policy.RequireClaim(ClaimTypes.Role));

var app = builder.Build();

app.UseCors();

if (app.Environment.IsDevelopment())
{
    app
        .UseSwagger()
        .UseSwaggerUI();
}
app.UseAuthentication();
app.UseAuthorization();

var quizzes = app.MapGroup("/quizzes");
var users = app.MapGroup("/users");

quizzes.MapGet("/", async (IQuizService service) =>
    await service.GetAsync());

quizzes.MapGet("/baseInfo", async (IQuizService service) =>
    await service.GetBaseInfoAsync());

quizzes.MapGet("/{id:length(24)}", async (string id, bool shuffle, IQuizService service) =>
    await service.GetAsync(id, shuffle)
        is Quiz quiz
            ? Results.Ok(quiz)
            : Results.NotFound());

quizzes.MapPost("/", async (Quiz newQuiz, IQuizService service) =>
{
    await service.InsertAsync(newQuiz);
    return Results.Created($"/quizzes/{newQuiz.Id}", newQuiz);
}).RequireAuthorization();

quizzes.MapDelete("/{id:length(24)}", async (string id, IQuizService service) =>
    await service.DeleteAsync(id)
        is true
            ? Results.NoContent()
            : Results.NotFound()).RequireAuthorization("Admin");

quizzes.MapDelete("/", async (IQuizService service) =>
{
    await service.DeleteAsync();
    return Results.NoContent();
}).RequireAuthorization("Admin");

users.MapGet("/", async (IUserService service) =>
    await service.GetAsync())
    .RequireAuthorization("Admin");

users.MapGet("/{id:length(24)}", async (string id, IUserService service) =>
    await service.GetByIdAsync(id)
        is UserDTO user
            ? Results.Ok(user)
            : Results.NotFound())
            .RequireAuthorization()
            .AddEndpointFilter<SameIdOrAdminFIlter>();

users.MapGet("/{id:length(24)}/role", async (string id, IUserService service) =>
    await service.GetUserRoleAsync(id)
        is EUserType role
            ? Results.Ok(role)
            : Results.NotFound())
            .RequireAuthorization()
            .AddEndpointFilter<SameIdOrAdminFIlter>();

users.MapPost("/", async (CreateUserDTO newUser, IUserService service) =>
{
    var (token, errors) = await service.CreateAsync(newUser);
    if (errors is null)
    {
        return Results.Ok(token);
    }
    return Results.ValidationProblem(errors);
});

users.MapDelete("/{id:length(24)}", async (string id, IUserService service) =>
    await service.DeleteAsync(id)
        is true
            ? Results.NoContent()
            : Results.NotFound())
            .RequireAuthorization()
            .AddEndpointFilter<SameIdOrAdminFIlter>();

users.MapDelete("/", async (IUserService service) =>
{
    await service.DeleteAsync();
    return Results.NoContent();
}).RequireAuthorization("Admin");

app.MapPost("/login", async (CredentialsDTO credentials, ILoginService service) =>
{
    var (token, errors) = await service.LogInAndGetTokenAsync(credentials);
    if (errors is null) return Results.Ok(token!);
    return Results.ValidationProblem(errors);
});

app.Run();