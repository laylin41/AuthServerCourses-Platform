using System.Security.Claims;
//using System.Security.Cryptography.X509Certificates;
using AuthServer;
using AuthServer.Data;
using AuthServer.Models;
using AuthServer.Services;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Test;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<CloudinaryService>();

builder.Services.AddAuthentication()
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://localhost:5005";
        options.TokenValidationParameters.ValidateAudience = false;
    });

builder.Services.AddAuthorization();

//var cert = new X509Certificate2("./keys/identity.pfx", "StrongP@ssw0rd!");
builder.Services.AddIdentityServer(options =>
    {
        options.Authentication.CookieLifetime = TimeSpan.FromHours(1);
    })
    .AddAspNetIdentity<ApplicationUser>()
    .AddInMemoryClients(Config.Clients)
    .AddInMemoryIdentityResources(Config.IdentityResources)
    .AddInMemoryApiScopes(Config.ApiScopes);

//builder.Services.AddDataProtection()
//    .PersistKeysToFileSystem(new DirectoryInfo("./keys"))
//    .SetApplicationName("CoursesAuthServer");

builder.Services.AddScoped<IProfileService, ProfileService>();

builder.Services.AddControllersWithViews();

//builder.WebHost.ConfigureKestrel(options =>
//{
//    var basePath = builder.Environment.ContentRootPath;
//    var certPath = Path.Combine(basePath, "certs", "localhost+2.pem");
//    var keyPath = Path.Combine(basePath, "certs", "localhost+2-key.pem");

//    if (File.Exists(certPath) && File.Exists(keyPath))
//    {
//        options.ListenLocalhost(5000, listenOptions =>
//        {
//            listenOptions.UseHttps(certPath, keyPath);
//        });
//        Console.WriteLine($"[Kestrel] HTTPS: using mkcert ({certPath})");
//    }
//    else
//    {
//        throw new FileNotFoundException($"Certificate not found: {certPath}");
//    }
//});

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseIdentityServer();
app.UseAuthorization();

app.MapDefaultControllerRoute();

await SeedData.EnsureSeedData(app.Services);

app.Run();
