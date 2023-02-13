using Identity;
using Identity.Configurations;
using Identity.Data;
using Identity.Endpoints;
using Identity.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Identity.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services,
        IConfigurationSection jwtOptionsSection,
        Action<DbContextOptionsBuilder> configureDatabaseAction)
    {
        services.Configure<JwtOptions>(jwtOptionsSection);
        services.AddDbContext<AppIdentityDbContext>(configureDatabaseAction);
        var securityKey = CoonfigureSignInCredentials(services, jwtOptionsSection);
        services.AddIdentity<IdentityUser, IdentityRole>(options => options.User.RequireUniqueEmail = true)
            .AddEntityFrameworkStores<AppIdentityDbContext>()
            .AddDefaultTokenProviders();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddTransient<IIdentitySeeder, IdentitySeeder>();
        ConfigureAuthenticationAndAuthorization(services, jwtOptionsSection, securityKey);
        return services;
    }

    public static WebApplication UseAuthenticationAndAuthorization(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }

    public static WebApplication UseIdentityModule(this WebApplication app)
    {
        EndpointsMapper.Map(app);
        return app;
    }

    private static void ConfigureAuthenticationAndAuthorization(IServiceCollection services, IConfigurationSection jwtOptionsSection, SymmetricSecurityKey securityKey)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = securityKey,
                ValidateIssuer = true,
                ValidIssuer = jwtOptionsSection.GetValue<string>("Issuer"),
                ValidateAudience = true,
                ValidAudience = jwtOptionsSection.GetValue<string>("Audience"),
                ValidateLifetime = true,
                RequireExpirationTime = true
            };
        });

        services.AddAuthorization(cfg =>
            cfg.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());
    }

    private static SymmetricSecurityKey CoonfigureSignInCredentials(IServiceCollection services, IConfigurationSection jwtOptionsSection)
    {
        var securityKeyConfig = jwtOptionsSection.GetValue<string>("SecurityKey");
        if(securityKeyConfig is null) throw new Exception("SecurityKey configuration needed");
        var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(securityKeyConfig));
        services.AddSingleton(
            _ => new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256));
        return securityKey;
    }
}
