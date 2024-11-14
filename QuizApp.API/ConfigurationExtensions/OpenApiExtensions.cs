using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace QuizApp.API.ConfigurationExtensions;

public static class OpenApiExtensions
{
    public static OpenApiOptions UseJwtBearerAuthentication(this OpenApiOptions options)
    {
        var scheme = new OpenApiSecurityScheme()
        {
            Name = JwtBearerDefaults.AuthenticationScheme,
            Type = SecuritySchemeType.Http,
            Scheme = JwtBearerDefaults.AuthenticationScheme,
            BearerFormat = "JWT",
            Description = """
                      JWT Authorization header using the Bearer scheme.<br>
                      Enter your token in the text input below.<br>
                      Example: '12345abcdef'
                      """,
            Reference = new()
            {
                Type = ReferenceType.SecurityScheme,
                Id = JwtBearerDefaults.AuthenticationScheme
            }
        };
        options.AddDocumentTransformer((doc, context, token) =>
        {
            doc.Components ??= new();
            doc.Components.SecuritySchemes.Add(JwtBearerDefaults.AuthenticationScheme, scheme);
            return Task.CompletedTask;
        });
        options.AddOperationTransformer((op, context, token) =>
        {
            if (context.Description.ActionDescriptor.EndpointMetadata.OfType<IAuthorizeData>().Any())
            {
                op.Security = [new() { [scheme] = [] }];
            }
            var req = new OpenApiSecurityRequirement();
            return Task.CompletedTask;
        });
        return options;
    }

    public static OpenApiOptions TreatEnumAsStrings(this OpenApiOptions options)
    {
        options.AddSchemaTransformer((schema, context, token) =>
        {
            var type = context.JsonTypeInfo.Type;
            if (type.IsEnum)
            {
                schema.Type = "string";
            }
            return Task.CompletedTask;
        });

        return options;
    }
}
