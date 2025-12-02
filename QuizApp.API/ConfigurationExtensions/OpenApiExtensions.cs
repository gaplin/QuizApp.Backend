using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

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
        };

        options.AddDocumentTransformer((doc, context, token) =>
        {
            doc.Components ??= new();
            doc.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
            {
                { JwtBearerDefaults.AuthenticationScheme, scheme }
            };
            return Task.CompletedTask;
        });
        options.AddOperationTransformer((op, context, token) =>
        {
            if (context.Description.ActionDescriptor.EndpointMetadata.OfType<IAuthorizeData>().Any())
            {
                op.Security = 
                [
                    new OpenApiSecurityRequirement() 
                    {
                        {
                            new OpenApiSecuritySchemeReference(scheme.Name),
                            []
                        }
                    }
                ];
            }
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
                schema.Type = JsonSchemaType.String;
            }
            return Task.CompletedTask;
        });

        return options;
    }
}
