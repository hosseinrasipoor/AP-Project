using Golestan.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityManager.Data;
using Golestan.ViewModels;
using System.Security.Claims;

namespace Golestan.Controllers
{
    [Authorize(Roles = "Instructor")]
    public class InstructorController : Controller
    {
        private readonly GolestanContext _context;

        public InstructorController(GolestanContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }


        // گرفتن آی‌دی استاد لاگین شده
        

        private async Task<int?> GetCurrentInstructorIdAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return null;

            if (!int.TryParse(userIdClaim.Value, out int userId))
                return null;

            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == userId);

            return instructor?.InstructorId;
        }

        // اکشن برای نمایش سشن‌های استاد
        public async Task<IActionResult> MySections()
        {
            int? instructorId = await GetCurrentInstructorIdAsync();
            if (instructorId == null)
                return Unauthorized();

            var sections = await _context.Teaches
                .Where(t => t.InstructorId == instructorId)
                .Include(t => t.Section)
                    .ThenInclude(s => s.Course)
                .Include(t => t.Section.Classroom)
                .Include(t => t.Section.TimeSlot)
                .Select(t => new SectionTableViewModel
                {
                    Id = t.Section.Id,
                    CourseTitle = t.Section.Course.Title,
                    InstructorName = $"{t.Instructor.User.FirstName} {t.Instructor.User.LastName}",
                    Year = t.Section.Year,
                    Semester = t.Section.Semester,
                    ClassroomName = $"{t.Section.Classroom.Building} - {t.Section.Classroom.RoomNumber}",
                    TimeSlotInfo = $"{t.Section.TimeSlot.Day} {t.Section.TimeSlot.StartTime:hh\\:mm} - {t.Section.TimeSlot.EndTime:hh\\:mm}",
                    FinalExamDate = t.Section.FinalExamDate
                })
                .ToListAsync();

            return View(sections);
        }

        // نمایش جزئیات یک سشن
        public async Task<IActionResult> SectionDetails(int id)
        {
            int? instructorId = await GetCurrentInstructorIdAsync();
            if (instructorId == null)
                return Unauthorized();

            var teach = await _context.Teaches
                .Include(t => t.Section)
                    .ThenInclude(s => s.Course)
                .Include(t => t.Section.Classroom)
                .Include(t => t.Section.TimeSlot)
                .Include(t => t.Section.Takes)
                    .ThenInclude(tk => tk.Student)
                        .ThenInclude(s => s.User)
                .Include(t => t.Instructor)
                    .ThenInclude(i => i.User)
                .FirstOrDefaultAsync(t => t.SectionId == id && t.InstructorId == instructorId);

            if (teach == null)
                return NotFound();

            var section = teach.Section;

            var model = new SectionDetailsViewModel
            {
                Id = section.Id,
                CourseTitle = section.Course.Title,
                Year = section.Year,
                Semester = section.Semester,
                ClassroomName = $"{section.Classroom.Building} {section.Classroom.RoomNumber}",
                TimeSlotInfo = $"{section.TimeSlot.Day} {section.TimeSlot.StartTime} - {section.TimeSlot.EndTime}",
                FinalExamDate = section.FinalExamDate,
                InstructorName = $"{teach.Instructor.User.FirstName} {teach.Instructor.User.LastName}",
                Students = section.Takes.Select(t => new StudentInSectionViewModel
                {
                    StudentId = t.StudentId,
                    StudentName = $"{t.Student.User.FirstName} {t.Student.User.LastName}",
                    Grade = t.Grade
                }).ToList()
            };

            return View(model);
        }

        // حذف دانشجو از سشن
        [HttpPost]
        public async Task<IActionResult> RemoveStudent(int sectionId, int studentId)
        {
            int? instructorId = await GetCurrentInstructorIdAsync();
            if (instructorId == null)
                return Unauthorized();

            var teach = await _context.Teaches
                .FirstOrDefaultAsync(t => t.SectionId == sectionId && t.InstructorId == instructorId);

            if (teach == null)
                return Forbid();

            var take = await _context.Takes
                .FirstOrDefaultAsync(t => t.SectionId == sectionId && t.StudentId == studentId);

            if (take == null)
                return NotFound();

            _context.Takes.Remove(take);
            await _context.SaveChangesAsync();

            return RedirectToAction("SectionDetails", new { id = sectionId });
        }

        // نمایش فرم ویرایش نمره
        [HttpGet]
        public async Task<IActionResult> UpdateGrade(int sectionId, int studentId)
        {
            int? instructorId = await GetCurrentInstructorIdAsync();
            if (instructorId == null)
                return Unauthorized();

            var teach = await _context.Teaches
                .FirstOrDefaultAsync(t => t.SectionId == sectionId && t.InstructorId == instructorId);

            if (teach == null)
                return Forbid();

            var take = await _context.Takes
                .Include(t => t.Student)
                    .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(t => t.SectionId == sectionId && t.StudentId == studentId);

            if (take == null)
                return NotFound();

            var model = new UpdateGradeViewModel
            {
                SectionId = sectionId,
                StudentId = studentId,
                Grade = take.Grade,
                StudentName = $"{take.Student.User.FirstName} {take.Student.User.LastName}"
            };

            return View(model);
        }

        // ثبت نمره
        [HttpPost]
        public async Task<IActionResult> UpdateGrade(UpdateGradeViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            int? instructorId = await GetCurrentInstructorIdAsync();
            if (instructorId == null)
                return Unauthorized();

            var teach = await _context.Teaches
                .FirstOrDefaultAsync(t => t.SectionId == model.SectionId && t.InstructorId == instructorId);

            if (teach == null)
                return Forbid();

            var take = await _context.Takes
                .FirstOrDefaultAsync(t => t.SectionId == model.SectionId && t.StudentId == model.StudentId);

            if (take == null)
                return NotFound();

            take.Grade = model.Grade;
            await _context.SaveChangesAsync();

            return RedirectToAction("SectionDetails", new { id = model.SectionId });
        }
    }
}
