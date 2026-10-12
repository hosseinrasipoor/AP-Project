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
            var profileIdClaim = User.FindFirst("ProfileId");
            if (profileIdClaim != null && int.TryParse(profileIdClaim.Value, out int profileId) && profileId > 0)
            {
                var profile = await _context.Students.FirstOrDefaultAsync(s => s.StudentId == profileId);
                if (profile != null) return profile.StudentId;
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return null;

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == userId);

            return student?.StudentId;
        }

        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return NotFound("کاربر یافت نشد");

            int? studentId = await GetCurrentStudentIdAsync();
            if (studentId == null)
                return NotFound("پروفایل دانشجویی یافت نشد");

            var takes = await _context.Takes
                .Include(t => t.Section)
                    .ThenInclude(s => s.Course)
                .Where(t => t.StudentId == studentId)
                .ToListAsync();

            var gradedTakes = takes.Where(t => t.Grade.HasValue).ToList();
            double gpa = gradedTakes.Any() ? Math.Round(gradedTakes.Average(t => (double)t.Grade!.Value), 2) : 0;

            var model = new StudentDashboardViewModel
            {
                StudentName = $"{user.FirstName} {user.LastName}",
                Courses = takes.Select(t => new StudentCourseViewModel
                {
                    CourseTitle = t.Section.Course.Title,
                    Grade = t.Grade.HasValue ? (double)t.Grade.Value : null
                }).ToList(),
                GPA = gpa
            };

            return View(model);
        }
    }
}
