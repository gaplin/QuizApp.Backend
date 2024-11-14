using Carter;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.IdentityModel.Tokens;
using QuizApp.API.ConfigurationExtensions;
using QuizApp.Domain;
using QuizApp.Infrastructure;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
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
    .AddOpenApi(options =>
    {
        options.UseJwtBearerAuthentication();
        options.TreatEnumAsStrings();
    })
    .AddMemoryCache()
    .AddInfrastructure(builder.Configuration.GetSection("QuizAppDatabase"))
    .AddDomain(builder.Configuration.GetSection("Jwt"))
    .AddCarter()
    .AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        {
            var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? Random.Shared.Next(0, 9).ToString();
            return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 1000,
                Window = TimeSpan.FromSeconds(10),
                QueueLimit = 10,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });
        });
    }
    );

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

app.UseRateLimiter();
app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(opts => opts.SwaggerEndpoint("/openapi/v1.json", "QuizApp.API"));
}
app.UseAuthentication();
app.UseAuthorization();

app.MapCarter();

app.Run();