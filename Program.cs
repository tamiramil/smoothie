using Microsoft.EntityFrameworkCore;
using smoothie.Data;

var builder = WebApplication.CreateBuilder(args);

string? constr = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services
    .AddDbContext<SmoothieContext>(options => options.UseSqlite(constr))
    .AddMvc();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();