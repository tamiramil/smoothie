using Microsoft.EntityFrameworkCore;
using smoothie.BLL.Services;
using Smoothie.BLL.Services;
using smoothie.DAL.Data;

var builder = WebApplication.CreateBuilder(args);

string? constr = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<SmoothieContext>(options => options.UseSqlite(constr));

builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();

builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    }
);

var app = builder.Build();

if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSession();
app.UseHttpsRedirection();
app.MapStaticAssets();
app.UseRouting();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}"
    )
    .WithStaticAssets();

using (var scope = app.Services.CreateScope()) {
    var context = scope.ServiceProvider.GetRequiredService<SmoothieContext>();
    context.Database.EnsureCreated();
}

app.Run();