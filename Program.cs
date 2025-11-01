using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Test;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using AuthServer;
using AuthServer.Services;
using AuthServer.Models;
using Microsoft.AspNetCore.Identity;
using Duende.IdentityServer.Services;
using AuthServer.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddIdentityServer(options =>
    {
        options.Authentication.CookieLifetime = TimeSpan.FromMinutes(30);
    })
    .AddAspNetIdentity<ApplicationUser>()
    .AddInMemoryClients(Config.Clients)
    .AddInMemoryIdentityResources(Config.IdentityResources)
    .AddInMemoryApiScopes(Config.ApiScopes);

builder.Services.AddScoped<IProfileService, ProfileService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseIdentityServer();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roles = new[] { "Admin", "Professor", "Student" };
    foreach (var r in roles)
    {
        if (!await roleManager.RoleExistsAsync(r))
            await roleManager.CreateAsync(new IdentityRole(r));
    }
}

app.MapDefaultControllerRoute();

await SeedData.EnsureSeedData(app.Services);

app.Run();
