using Microsoft.EntityFrameworkCore;

namespace Products;

public class ProductsDbContex : DbContext
{
    public DbSet<Product> Products => Set<Product>();

    public ProductsDbContex(DbContextOptions<ProductsDbContex> options) : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Product>().HasKey(x => x.Id);
        builder.Entity<Product>().Property(x => x.Id)
            .ValueGeneratedNever();
    }
}