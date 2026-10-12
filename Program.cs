using Microsoft.EntityFrameworkCore;
using UniversityManager.Data;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<GolestanContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddAuthentication("MyCookieAuth")
    .AddCookie("MyCookieAuth", options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();  
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<GolestanContext>();
        if (context.Database.CanConnect())
        {
            if (!context.Roles.Any())
            {
                context.Roles.AddRange(
                    new Golestan.Models.Role { Id = 1, Name = Golestan.Models.RoleType.Admin },
                    new Golestan.Models.Role { Id = 2, Name = Golestan.Models.RoleType.Student },
                    new Golestan.Models.Role { Id = 3, Name = Golestan.Models.RoleType.Instructor }
                );
                context.SaveChanges();
            }

            if (!context.Users.Any())
            {
                var adminUser = new Golestan.Models.User
                {
                    FirstName = "مدیر",
                    LastName = "سیستم",
                    Email = "admin@golestan.ir",
                    HashedPassword = "admin",
                    CreatedAt = DateTime.Now
                };
                context.Users.Add(adminUser);
                context.SaveChanges();

                context.UserRoles.Add(new Golestan.Models.UserRole
                {
                    UserId = adminUser.Id,
                    RoleId = (int)Golestan.Models.RoleType.Admin
                });
                context.SaveChanges();
            }
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Could not seed initial database data automatically.");
    }
}

app.Run();
