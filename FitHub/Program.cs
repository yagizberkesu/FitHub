using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using FitHub.Models; // For session extensions  

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<FitHubContext>(options =>
 options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Add session services  and configure session options
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews(); // No global filters, keep it simple  

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Enable session middleware  
app.UseSession();

app.MapControllerRoute(
 name: "default",
 pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
