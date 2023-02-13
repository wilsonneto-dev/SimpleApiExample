using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Products;
public static class Extensions
{
    public static IServiceCollection AddProductsModule(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDatabaseAction)
    {
        services.AddDbContext<ProductsDbContex>(configureDatabaseAction);
        services.AddScoped<ProductRepository>();
        return services;
    }

    public static void UseProductsModule(this WebApplication app) => Endpoints.MapEndpoints(app);
}