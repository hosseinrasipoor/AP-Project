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
        public IActionResult CreateUser(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (_context.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "A user with this email already exists.");
                return View(model);
            }

            var user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                HashedPassword = model.Password,
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

        public IActionResult StudentTable()
        {
            var students = _context.Students
                .Include(s => s.User)
                .Select(s => new StudentViewModel
                {
                    StudentId = s.StudentId,
                    UserId = s.UserId,
                    FullName = $"{s.User.FirstName} {s.User.LastName}",
                    Email = s.User.Email,
                    EnrollmentDate = s.EnrollmentDate
                }).ToList();

            return View(students);
        }


        [HttpGet]
        public IActionResult CreateStudent()
        {
            ViewBag.Users = _context.Users
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = $"{u.FirstName} {u.LastName} - {u.Email}"
                }).ToList();

            return View();
        }

        [HttpPost]
        public IActionResult CreateStudent(CreateStudentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Users = _context.Users
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id.ToString(),
                        Text = $"{u.FirstName} {u.LastName} - {u.Email}"
                    }).ToList();

                return View(model);
            }

            var student = new Student
            {
                UserId = model.UserId,
                EnrollmentDate = model.EnrollmentDate
            };

            _context.Students.Add(student);
            _context.SaveChanges();

            return RedirectToAction("StudentTable");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentId == id);
            if (student == null)
                return NotFound();

            var takes = _context.Takes.Where(t => t.StudentId == id);

            _context.Takes.RemoveRange(takes);

            _context.Students.Remove(student);

            await _context.SaveChangesAsync();

            return RedirectToAction("StudentTable");
        }


        [HttpGet]
        public IActionResult CreateInstructor()
        {
            ViewBag.Users = _context.Users
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = $"{u.FirstName} {u.LastName} - {u.Email}"
                }).ToList();

            return View();
        }

        [HttpPost]
        public IActionResult CreateInstructor(CreateInstructorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Users = _context.Users
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id.ToString(),
                        Text = $"{u.FirstName} {u.LastName} - {u.Email}"
                    }).ToList();

                return View(model);
            }

            var instructor = new Instructor
            {
                UserId = model.UserId,
                Salary = model.Salary,
                HireDate = model.HireDate
            };

            _context.Instructors.Add(instructor);
            _context.SaveChanges();

            return RedirectToAction("InstructorTable");

        }

        public IActionResult InstructorTable()
        {
            var instructors = _context.Instructors
                .Include(i => i.User)
                .Select(i => new InstructorViewModel
                {
                    InstructorId = i.InstructorId,
                    UserId = i.UserId,
                    FullName = $"{i.User.FirstName} {i.User.LastName}",
                    Email = i.User.Email,
                    Salary = i.Salary,
                    HireDate = i.HireDate
                }).ToList();

            return View(instructors);
        }

        public async Task<IActionResult> DeleteInstructor(int id)
        {
            var instructor = await _context.Instructors.FirstOrDefaultAsync(i => i.InstructorId == id);
            if (instructor == null)
                return NotFound();

            
            var teaches = _context.Teaches.Where(t => t.InstructorId == id);

            _context.Teaches.RemoveRange(teaches);

            _context.Instructors.Remove(instructor);

            await _context.SaveChangesAsync();

            return RedirectToAction("InstructorTable");
        }


        
        public IActionResult CreateCourse()
        {
            return View();
        }

        
        [HttpPost]
        public async Task<IActionResult> CreateCourse(CreateCourseViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var course = new Course
            {
                Title = model.Title,
                Code = model.Code,
                Unit = model.Unit,
                Description = model.Description,
                FinalExamDate = model.FinalExamDate
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return RedirectToAction("CourseTable");
        }

        public IActionResult CourseTable()
        {
            var courses = _context.Courses.ToList();
            return View(courses);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Takes)
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Teach)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
                return NotFound();

            foreach (var section in course.Sections)
            {
                
                if (section.Takes != null && section.Takes.Any())
                    _context.Takes.RemoveRange(section.Takes);

                
                if (section.Teach != null)
                    _context.Teaches.Remove(section.Teach);
            }

            
            _context.Sections.RemoveRange(course.Sections);

            
            _context.Courses.Remove(course);

            await _context.SaveChangesAsync();

            return RedirectToAction("CourseTable");
        }



    }
}
