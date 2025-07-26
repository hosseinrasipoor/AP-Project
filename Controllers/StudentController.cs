using Golestan.Models;
using Golestan.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityManager.Data;
using System.Security.Claims;

namespace Golestan.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly GolestanContext _context;

        public StudentController(GolestanContext context)
        {
            _context = context;
        }

        private async Task<int?> GetCurrentStudentIdAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return null;

            if (!int.TryParse(userIdClaim.Value, out int userId))
                return null;

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == userId);

            return student?.StudentId;
        }

        
        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            if (!int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return NotFound("User not found");

            int? studentId = await GetCurrentStudentIdAsync();
            if (studentId == null)
                return NotFound("Student profile not found");

            var takes = await _context.Takes
                .Include(t => t.Section)
                    .ThenInclude(s => s.Course)
                .Where(t => t.StudentId == studentId)
                .ToListAsync();

            var model = new StudentDashboardViewModel
            {
                StudentName = $"{user.FirstName} {user.LastName}",
                Courses = takes.Select(t => new StudentCourseViewModel
                {
                    CourseTitle = t.Section.Course.Title,
                    Grade = t.Grade
                }).ToList(),
                GPA = takes.Any() ? Math.Round(takes.Average(t => t.Grade), 2) : 0
            };

            return View(model);
        }
    }
}
