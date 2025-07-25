using Microsoft.EntityFrameworkCore;
using UniversityManager.Data;
using Golestan.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        IMvcBuilder mvcBuilder = builder.Services.AddControllersWithViews(); // اصلاح: AddControlUersWithViews → AddControllersWithViews

        builder.Services.AddDbContext<GolestanContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))); // اصلاح: UsesqlServer → UseSqlServer

        builder.Services.AddDefaultIdentity<SampleUser>(options => 
{
        options.SignIn.RequireConfirmedAccount = true;
    
    // Disable new user registrations
        options.User.AllowedUserNameCharacters = ""; // Prevent new usernames
        })
        .AddEntityFrameworkStores<GolestanContext>();

        var app = builder.Build(); // اصلاح کامل این خط

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error"); // اصلاح: UsesExceptionHandler → UseExceptionHandler
            app.UseHsts(); // افزودن HSTS اجباری
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication(); // افزودن احراز هویت
        app.UseAuthorization();  // افزودن مجوزدهی

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.MapRazorPages(); // ضروری برای Identity

        app.Run();
    }
}