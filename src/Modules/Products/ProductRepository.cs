// using Bogus;

using Microsoft.EntityFrameworkCore;

namespace Products;

public class ProductRepository
{
    private readonly ProductsDbContex _context;
    private DbSet<Product> _products => _context.Set<Product>();

    public ProductRepository(ProductsDbContex context) => _context = context;

    public List<Product> GetProducts(int page = 1, int pageSize = 10, string? query = "") =>
        _products
            .Where(x => string.IsNullOrWhiteSpace(query) || x.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

    public Product? GetProduct(Guid id) => _products.FirstOrDefault(p => p.Id == id);

    public void CreateProduct(Product product)
    {
        _products.Add(product);
        _context.SaveChanges();
    }

    public void UpdateProduct(Product product)
    {
        _products.Update(product);
        _context.SaveChanges();
    }

    public void DeleteProduct(Product product)
    {
        _products.Remove(product);
        _context.SaveChanges();
    }
}
