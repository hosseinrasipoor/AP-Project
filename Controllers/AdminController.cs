using Golestan.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniversityManager.Data;
using Golestan.ViewModels;

namespace Golestan.Controllers
{
    public class AdminController : Controller
    {
        private readonly GolestanContext _context;

        public AdminController(GolestanContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CreateUser()
        {
            return View();
        }



        [HttpPost]
        public IActionResult CreateUser(string FirstName, string LastName, string Email, string HashedPassword)
        {
            var exsitingUser = _context.Users.FirstOrDefault(u => u.Email == Email);

            if (exsitingUser != null)
            {
                ViewBag.ErrorMessage = " We already have user with that emailaddress!";
                return View();
            }
            var user = new User
            {
                FirstName = FirstName,
                LastName = LastName,
                Email = Email,
                HashedPassword = HashedPassword,
                CreatedAt = DateTime.Now,
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return RedirectToAction("UserTable");
        }

        public IActionResult UserTable()
        {
            var users = _context.Users.Include(users => users.UserRoles).ThenInclude(userRole => userRole.Role).ToList();
            return View(users);

        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .Include(u => u.StudentProfiles)
                .Include(u => u.InstructorProfiles)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

            
            _context.UserRoles.RemoveRange(user.UserRoles);
            _context.Students.RemoveRange(user.StudentProfiles);
            _context.Instructors.RemoveRange(user.InstructorProfiles);
            _context.Users.Remove(user);

            await _context.SaveChangesAsync();

            return RedirectToAction("UserTable");
        }
    }
}
