using Golestan.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniversityManager.Data;

namespace Golestan.Controllers
{
    public class AdminController : Controller
    {
        private readonly GolestanContext _context;

        public AdminController(GolestanContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).ToListAsync();

            return View(users);
        }

        public IActionResult Create()
        {
            ViewBag.Roles = new SelectList(_context.Roles, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user, int[] selectedRoles, bool isStudent, bool isInstructor)
        {
            if (ModelState.IsValid)
            {
                user.CreatedAt = DateTime.Now;
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                foreach (var roleId in selectedRoles)
                {
                    var userRole = new UserRole
                    {
                        UserId = user.Id,
                        RoleId = roleId
                    };
                    _context.UserRoles.Add(userRole);
                }

                if (isStudent)
                {
                    var student = new Student
                    {
                        UserId = user.Id,
                        EnrollmentDate = DateTime.Now
                    };
                    _context.Students.Add(student);
                }

                if (isInstructor)
                {
                    var instructor = new Instructor
                    {
                        UserId = user.Id,
                        Salary = 0,
                        HireDate = DateTime.Now
                    };
                    _context.Instructors.Add(instructor);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Roles = new SelectList(_context.Roles, "Id", "Name");
            return View(user);
        }
    }
}
