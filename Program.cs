using Microsoft.EntityFrameworkCore;
using smoothie.Data;

var builder = WebApplication.CreateBuilder(args);

string? constr = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddMvc();
builder.Services.AddDbContext<SmoothieContext>(options => options.UseSqlite(constr));

var app = builder.Build();

app.UseSession();
app.UseHttpsRedirection();
app.MapStaticAssets();
app.UseRouting();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Projects}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();