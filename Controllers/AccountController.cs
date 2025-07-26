using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UniversityManager.Data;
using Golestan.ViewModels.Account;
using Golestan.Models;

namespace Golestan.Controllers
{
    public class AccountController : Controller
    {
        private readonly GolestanContext _context;

        public AccountController(GolestanContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        private async Task<IActionResult> LoginWithProfile(User user, RoleType role, int profileId)
        {
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, role.ToString()),
        new Claim("ProfileId", profileId.ToString())
    };

            var identity = new ClaimsIdentity(claims, "MyCookieAuth");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("MyCookieAuth", principal);

            switch (role)
            {
                case RoleType.Admin:
                    return RedirectToAction("Index", "Admin");
                case RoleType.Instructor:
                    return RedirectToAction("Index", "Instructor");
                case RoleType.Student:
                    return RedirectToAction("Index", "Student");
                default:
                    return RedirectToAction("Index", "Home");
            }
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }



        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            User? 
                 user1 = await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(u => u.StudentProfiles)
                .Include(u => u.InstructorProfiles)
                .FirstOrDefaultAsync(u => u.Email == model.Email);
            var user = user1;

            if (user == null || user.HashedPassword != model.Password)
            {
                ModelState.AddModelError(string.Empty, "ایمیل یا رمز عبور اشتباه است.");
                return View(model);
            }

            int totalProfiles = (user.StudentProfiles?.Count ?? 0) + (user.InstructorProfiles?.Count ?? 0);

            // فقط یک نقش-پروفایل داره → مستقیم لاگین شه
            if (totalProfiles == 1)
            {
                if (user.StudentProfiles?.Count == 1)
                {
                    return await LoginWithProfile(user, RoleType.Student, user.StudentProfiles.First().StudentId);
                }
                else
                {
                    return await LoginWithProfile(user, RoleType.Instructor, user.InstructorProfiles.First().InstructorId);
                }
            }

            // چند نقش داره → بفرستش انتخاب نقش
            var roleOptions = new List<RoleSelectionViewModel>();

            if (user.StudentProfiles != null)
            {
                foreach (var s in user.StudentProfiles)
                {
                    roleOptions.Add(new RoleSelectionViewModel
                    {
                        ProfileId = s.StudentId,
                        Role = RoleType.Student,
                        DisplayName = $"دانشجو - {user.FirstName} {user.LastName}"
                    });
                }
            }

            if (user.InstructorProfiles != null)
            {
                foreach (var i in user.InstructorProfiles)
                {
                    roleOptions.Add(new RoleSelectionViewModel
                    {
                        ProfileId = i.InstructorId,
                        Role = RoleType.Instructor,
                        DisplayName = $"استاد - {user.FirstName} {user.LastName}"
                    });
                }
            }

            if (user.UserRoles.Any(ur => ur.RoleId== 1))
            {
                roleOptions.Add(new RoleSelectionViewModel
                {
                    ProfileId = 0, // یا هر مقدار مناسبی که تو سیستم تعریف کردی
                    Role = RoleType.Admin,
                    DisplayName = $"ادمین - {user.FirstName} {user.LastName}"
                });
            }

            TempData["UserId"] = user.Id;
            return View("SelectRole", roleOptions);
        }


        public IActionResult AccessDenied()
        {
            return View();
        }


        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("MyCookieAuth");
            return RedirectToAction("Login", "Account");
        }


       

        [HttpPost]
        public async Task<IActionResult> SelectRole(int SelectedProfileId, [FromForm] Dictionary<int, RoleType> Roles)
        {
            if (TempData["UserId"] == null)
                return RedirectToAction("Login");

            int userId = (int)TempData["UserId"];
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return RedirectToAction("Login");

            var role = Roles[SelectedProfileId];
            return await LoginWithProfile(user, role, SelectedProfileId);
        }


    }
}
