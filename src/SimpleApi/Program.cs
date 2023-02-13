using Identity.Data;
using Identity.Extensions;
using Microsoft.EntityFrameworkCore;
using Products;
using SimpleApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAndConfigureSwagger();
builder.Services.AddIdentityModule(
    builder.Configuration.GetSection("JwtOptions"),
    options => options.UseInMemoryDatabase("IdentityDb"));
builder.Services.ConfigureCors(builder.Configuration.GetValue<string>("Cors:AllowedOrigins"));
builder.Services.AddProductsModule(options => options.UseInMemoryDatabase("ProductsDb"));

var app = builder.Build();
await app.SeedIdentity();

app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseExceptionsMiddleware();
app.UseAuthenticationAndAuthorization();
app.UseIdentityModule();
app.UseProductsModule();

app.Run();
