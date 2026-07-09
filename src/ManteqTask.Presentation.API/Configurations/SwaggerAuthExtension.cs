using Microsoft.OpenApi;

namespace ManteqTask.Presentation.API.Configurations;

public static class SwaggerAuthExtension
{
    /// <summary>
    /// Adds a JWT bearer security scheme to Swagger so the UI shows an "Authorize" button
    /// where a token can be entered and sent on subsequent requests.
    /// </summary>
    public static IServiceCollection AddSwaggerAuth(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Enter your JWT access token (the 'Bearer ' prefix is added automatically).",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "bearer"
            });

            c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer", document),
                    new List<string>()
                }
            });
        });

        return services;
    }
}
