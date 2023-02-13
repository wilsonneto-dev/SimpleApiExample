using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Products;

internal static class Endpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/products", (int? page, int? pageSize, string? query, ProductRepository repository) =>
            TypedResults.Ok(repository.GetProducts(page ?? 0, pageSize ?? 10, query)))
            .WithOpenApi()
            .WithTags("Products")
            .AllowAnonymous()
            .WithName("ListProducts");
        
        endpoints.MapGet("/products/{id:guid}", Results<Ok<Product>, NotFound> (Guid id, ProductRepository repository) =>
        {
            if(repository.GetProduct(id) is Product product) return TypedResults.Ok(product);
            return TypedResults.NotFound();
        })
            .WithOpenApi()
            .WithTags("Products")
            .WithName("ProductDetails")
            .AllowAnonymous();

        endpoints.MapPost("/products", (ProductInput input, ProductRepository repository) =>
        {
            var product = new Product(input.Name, input.Description, input.Price);
            repository.CreateProduct(product);
            return TypedResults.CreatedAtRoute(product, "ProductDetails", new { id = product.Id });
        })
            .WithOpenApi()
            .WithTags("Products")
            .WithName("AddProduct");

        endpoints.MapPut("/products/{id:guid}", Results<NoContent, NotFound> (
            Guid id, 
            ProductInput input, 
            ProductRepository repository) =>
        {
            var product = repository.GetProduct(id);
            if(product is null)
                return TypedResults.NotFound();
            product.Update(input.Name, input.Description, input.Price);
            repository.UpdateProduct(product);
            return TypedResults.NoContent();
        })
            .WithOpenApi()
            .WithTags("Products")
            .WithName("UpdateProduct");

        endpoints.MapDelete("/products/{id:guid}", Results<NoContent, NotFound> (Guid id, ProductRepository repository) =>
        {
            var product = repository.GetProduct(id);
            if(product is null) return TypedResults.NotFound();
            repository.DeleteProduct(product);
            return TypedResults.NoContent();
        })
            .WithOpenApi()
            .WithTags("Products")
            .WithName("DeleteProduct");
    }

    public record ProductInput(string Name, string Description, decimal Price);
}
