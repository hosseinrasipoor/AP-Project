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
            return RedirectToAction("Login");
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
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin")) return RedirectToAction("Index", "Admin");
                if (User.IsInRole("Instructor")) return RedirectToAction("Index", "Instructor");
                if (User.IsInRole("Student")) return RedirectToAction("Index", "Student");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            User? user = await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(u => u.StudentProfiles)
                .Include(u => u.InstructorProfiles)
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null || user.HashedPassword != model.Password)
            {
                ModelState.AddModelError(string.Empty, "ایمیل یا رمز عبور اشتباه است.");
                return View(model);
            }

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

            if (user.UserRoles != null && user.UserRoles.Any(ur => ur.RoleId == (int)RoleType.Admin))
            {
                roleOptions.Add(new RoleSelectionViewModel
                {
                    ProfileId = 0,
                    Role = RoleType.Admin,
                    DisplayName = $"مدیریت - {user.FirstName} {user.LastName}"
                });
            }

            if (roleOptions.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "حساب کاربری شما هنوز نقشی در سامانه ندارد. لطفاً با مدیر سیستم تماس بگیرید.");
                return View(model);
            }

            // If user only has a single role, sign them in directly
            if (roleOptions.Count == 1)
            {
                return await LoginWithProfile(user, roleOptions[0].Role, roleOptions[0].ProfileId);
            }

            TempData["UserId"] = user.Id;
            return View("SelectRole", roleOptions);
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("MyCookieAuth");
            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectRole(int SelectedProfileId, [FromForm] Dictionary<int, RoleType> Roles)
        {
            if (TempData["UserId"] == null)
                return RedirectToAction("Login");

            int userId = (int)TempData["UserId"];
            TempData.Keep("UserId");

            var user = await _context.Users
                .Include(u => u.UserRoles)
                .Include(u => u.StudentProfiles)
                .Include(u => u.InstructorProfiles)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return RedirectToAction("Login");

            if (Roles == null || !Roles.TryGetValue(SelectedProfileId, out var requestedRole))
                return RedirectToAction("Login");

            // Security check: verify user actually owns the selected role and profile
            bool isAuthorized = false;
            if (requestedRole == RoleType.Admin)
            {
                isAuthorized = user.UserRoles != null && user.UserRoles.Any(ur => ur.RoleId == (int)RoleType.Admin);
            }
            else if (requestedRole == RoleType.Instructor)
            {
                isAuthorized = user.InstructorProfiles != null && user.InstructorProfiles.Any(i => i.InstructorId == SelectedProfileId);
            }
            else if (requestedRole == RoleType.Student)
            {
                isAuthorized = user.StudentProfiles != null && user.StudentProfiles.Any(s => s.StudentId == SelectedProfileId);
            }

            if (!isAuthorized)
            {
                return Forbid();
            }

            return await LoginWithProfile(user, requestedRole, SelectedProfileId);
        }
    }
}
