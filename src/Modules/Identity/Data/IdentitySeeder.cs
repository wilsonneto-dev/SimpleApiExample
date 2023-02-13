using Identity.Constants;
using Identity.Exceptions;
using Identity.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Identity.Data;

internal class IdentitySeeder : IIdentitySeeder
{
    private readonly AppIdentityDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public IdentitySeeder(
        AppIdentityDbContext context,
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task Initialize()
    {
        _context.Database.EnsureDeleted();
        if(_context.Database.EnsureCreated())
        {
            await CreateDefaultRoles();
            await CreateDefaultUsers();
        }
    }

    private async Task CreateDefaultUsers()
    {
        var defaultUsers = new List<IdentityUser>() {
            new() { UserName = "Tester", Email = "one@tester.io", EmailConfirmed = true }};
        foreach(var user in defaultUsers)
            await CreateUser(user, "P4SsW0RD@.", Roles.User);
    }

    private async Task CreateDefaultRoles()
    {
        var roleChatUserExists = await _roleManager.RoleExistsAsync(Roles.User);
        if(!roleChatUserExists)
        {
            var result = await _roleManager.CreateAsync(new IdentityRole(Roles.User));
            if(!result.Succeeded)
                throw new IdentityException($"Error creating role {Roles.User}.");
        }
    }

    private async Task CreateUser(IdentityUser user, string password, string? initialRole = null)
    {
        var userFound = await _userManager.FindByNameAsync(user.UserName);
        if(userFound is null)
        {
            var result = await _userManager.CreateAsync(user, password);
            if(result.Succeeded && !string.IsNullOrWhiteSpace(initialRole))
                await _userManager.AddToRoleAsync(user, initialRole);
        }
    }
}

public static class IdentitySeederServiceCollection
{
    public static Task SeedIdentity(this IHost app)
        => app.Services.CreateScope().ServiceProvider
            .GetRequiredService<IIdentitySeeder>().Initialize();
}