using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SimpleApi;

internal static class CorsConfigurationExtensions
{
    public static IServiceCollection ConfigureCors(
        this IServiceCollection services,
        string? origins
    ) => services.AddCors(options =>
    {
        options.AddDefaultPolicy(policyBuilder =>
        {
            if(origins is null) throw new Exception("Configuration Cors:AllowedOrigins needed");
            var originsArray = origins.Split(',').Select(x => x.Trim()).ToArray();
            policyBuilder
                .WithOrigins(originsArray)
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
    });
}

internal static class SwaggerConfigurationExtensions
{
    public static IServiceCollection AddAndConfigureSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options => {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "APi", Version = "v1" });
            options.AddSecurityDefinitions();
        });
        return services;
    }

    private static void AddSecurityDefinitions(this SwaggerGenOptions options)
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = @"JWT Authorization header using the Bearer scheme. 
                        Enter 'Bearer' [space] and then your token in the text input below. 
                        Example: 'Bearer 12345abcdef'",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement()
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    },
                    Scheme = "oauth2",
                    Name = "Bearer",
                    In = ParameterLocation.Header,
                },
                new List<string>()
            }
        });
    }
}
