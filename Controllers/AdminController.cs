using Golestan.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UniversityManager.Data;
using Golestan.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace Golestan.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly GolestanContext _context;

        public AdminController(GolestanContext context)
        {
            _context = context;
        }

        private async Task AssignRoleToUserAsync(int userId, RoleType role)
        {
            bool alreadyHasRole = await _context.UserRoles
                .AnyAsync(ur => ur.UserId == userId && ur.RoleId == (int)role);

            if (!alreadyHasRole)
            {
                _context.UserRoles.Add(new UserRole
                {
                    UserId = userId,
                    RoleId = (int)role
                });

                await _context.SaveChangesAsync();
            }
        }

        private void FillDropDowns(CreateSectionViewModel model)
        {
            model.Courses = _context.Courses.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.Title} ({c.Code})"
            }).ToList();

            model.Classrooms = _context.Classrooms.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.Building} - اتاق {c.RoomNumber} (ظرفیت: {c.Capacity})"
            }).ToList();

            model.TimeSlots.Clear();
            var slots = _context.TimeSlots.ToList();
            model.TimeSlots = slots.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = $"{t.Day} ({t.StartTime:HH\\:mm} تا {t.EndTime:HH\\:mm})"
            }).ToList();

            model.Instructors = _context.Instructors.Include(i => i.User).Select(i => new SelectListItem
            {
                Value = i.InstructorId.ToString(),
                Text = $"{i.User.FirstName} {i.User.LastName}"
            }).ToList();
        }

        public IActionResult Index()
        {
            return View();
        }

        // ==================== USER MANAGEMENT ====================

        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "کاربری با این نشانی ایمیل قبلاً در سامانه ثبت شده است.");
                return View(model);
            }

            var user = new User
            {
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                Email = model.Email.Trim().ToLower(),
                HashedPassword = model.Password,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("UserTable");
        }

        public async Task<IActionResult> UserTable()
        {
            var users = await _context.Users
                .Include(u => u.UserRoles)!
                    .ThenInclude(ur => ur.Role)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .Include(u => u.StudentProfiles)
                .Include(u => u.InstructorProfiles)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

            // 1. Remove Takes of student profiles belonging to this user
            var studentIds = user.StudentProfiles?.Select(s => s.StudentId).ToList() ?? new List<int>();
            if (studentIds.Any())
            {
                var studentTakes = _context.Takes.Where(t => studentIds.Contains(t.StudentId));
                _context.Takes.RemoveRange(studentTakes);
            }

            // 2. Remove Teaches of instructor profiles belonging to this user
            var instructorIds = user.InstructorProfiles?.Select(i => i.InstructorId).ToList() ?? new List<int>();
            if (instructorIds.Any())
            {
                var instructorTeaches = _context.Teaches.Where(t => instructorIds.Contains(t.InstructorId));
                _context.Teaches.RemoveRange(instructorTeaches);
            }

            // 3. Remove roles and profiles
            if (user.UserRoles != null)
                _context.UserRoles.RemoveRange(user.UserRoles);

            if (user.StudentProfiles != null)
                _context.Students.RemoveRange(user.StudentProfiles);

            if (user.InstructorProfiles != null)
                _context.Instructors.RemoveRange(user.InstructorProfiles);

            // 4. Remove the user entity
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("UserTable");
        }

        // ==================== STUDENT MANAGEMENT ====================

        public async Task<IActionResult> StudentTable()
        {
            var students = await _context.Students
                .Include(s => s.User)
                .Select(s => new StudentViewModel
                {
                    StudentId = s.StudentId,
                    UserId = s.UserId,
                    FullName = $"{s.User.FirstName} {s.User.LastName}",
                    Email = s.User.Email,
                    EnrollmentDate = s.EnrollmentDate
                })
                .ToListAsync();

            return View(students);
        }

        [HttpGet]
        public async Task<IActionResult> CreateStudent()
        {
            ViewBag.Users = await _context.Users
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = $"{u.FirstName} {u.LastName} - {u.Email}"
                }).ToListAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStudent(CreateStudentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Users = await _context.Users
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id.ToString(),
                        Text = $"{u.FirstName} {u.LastName} - {u.Email}"
                    }).ToListAsync();

                return View(model);
            }

            // Check if user already has a student profile
            bool alreadyStudent = await _context.Students.AnyAsync(s => s.UserId == model.UserId);
            if (alreadyStudent)
            {
                ModelState.AddModelError("UserId", "این کاربر در حال حاضر دارای پرونده دانشجویی فعال است.");
                ViewBag.Users = await _context.Users
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id.ToString(),
                        Text = $"{u.FirstName} {u.LastName} - {u.Email}"
                    }).ToListAsync();
                return View(model);
            }

            var student = new Student
            {
                UserId = model.UserId,
                EnrollmentDate = model.EnrollmentDate
            };

            await AssignRoleToUserAsync(model.UserId, RoleType.Student);
            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return RedirectToAction("StudentTable");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.StudentId == id);

            if (student == null)
                return NotFound();

            var takes = _context.Takes.Where(t => t.StudentId == id);
            _context.Takes.RemoveRange(takes);

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();

            bool hasOtherStudentProfiles = await _context.Students
                .AnyAsync(s => s.UserId == student.UserId && s.StudentId != id);

            if (!hasOtherStudentProfiles)
            {
                var userRole = await _context.UserRoles
                    .FirstOrDefaultAsync(ur => ur.UserId == student.UserId && ur.RoleId == (int)RoleType.Student);

                if (userRole != null)
                {
                    _context.UserRoles.Remove(userRole);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("StudentTable");
        }

        // ==================== INSTRUCTOR MANAGEMENT ====================

        public async Task<IActionResult> InstructorTable()
        {
            var instructors = await _context.Instructors
                .Include(i => i.User)
                .Select(i => new InstructorViewModel
                {
                    InstructorId = i.InstructorId,
                    UserId = i.UserId,
                    FullName = $"{i.User.FirstName} {i.User.LastName}",
                    Email = i.User.Email,
                    Salary = i.Salary,
                    HireDate = i.HireDate
                })
                .ToListAsync();

            return View(instructors);
        }

        [HttpGet]
        public async Task<IActionResult> CreateInstructor()
        {
            ViewBag.Users = await _context.Users
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = $"{u.FirstName} {u.LastName} - {u.Email}"
                }).ToListAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateInstructor(CreateInstructorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Users = await _context.Users
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id.ToString(),
                        Text = $"{u.FirstName} {u.LastName} - {u.Email}"
                    }).ToListAsync();

                return View(model);
            }

            // Check if user already has an instructor profile
            bool alreadyInstructor = await _context.Instructors.AnyAsync(i => i.UserId == model.UserId);
            if (alreadyInstructor)
            {
                ModelState.AddModelError("UserId", "این کاربر در حال حاضر دارای پرونده استادی فعال است.");
                ViewBag.Users = await _context.Users
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id.ToString(),
                        Text = $"{u.FirstName} {u.LastName} - {u.Email}"
                    }).ToListAsync();
                return View(model);
            }

            var instructor = new Instructor
            {
                UserId = model.UserId,
                Salary = model.Salary,
                HireDate = model.HireDate
            };

            await AssignRoleToUserAsync(model.UserId, RoleType.Instructor);
            _context.Instructors.Add(instructor);
            await _context.SaveChangesAsync();

            return RedirectToAction("InstructorTable");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteInstructor(int id)
        {
            var instructor = await _context.Instructors
                .Include(i => i.User)
                .FirstOrDefaultAsync(i => i.InstructorId == id);

            if (instructor == null)
                return NotFound();

            var teaches = _context.Teaches.Where(t => t.InstructorId == id);
            _context.Teaches.RemoveRange(teaches);

            _context.Instructors.Remove(instructor);
            await _context.SaveChangesAsync();

            bool hasOtherInstructorProfiles = await _context.Instructors
                .AnyAsync(i => i.UserId == instructor.UserId && i.InstructorId != id);

            if (!hasOtherInstructorProfiles)
            {
                var userRole = await _context.UserRoles
                    .FirstOrDefaultAsync(ur => ur.UserId == instructor.UserId && ur.RoleId == (int)RoleType.Instructor);

                if (userRole != null)
                {
                    _context.UserRoles.Remove(userRole);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("InstructorTable");
        }

        // ==================== COURSE MANAGEMENT ====================

        public async Task<IActionResult> CourseTable()
        {
            var courses = await _context.Courses.ToListAsync();
            return View(courses);
        }

        [HttpGet]
        public IActionResult CreateCourse()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(CreateCourseViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            bool codeExists = await _context.Courses.AnyAsync(c => c.Code == model.Code.Trim());
            if (codeExists)
            {
                ModelState.AddModelError("Code", "درسی با این کد در سامانه ثبت شده است.");
                return View(model);
            }

            var course = new Course
            {
                Title = model.Title.Trim(),
                Code = model.Code.Trim(),
                Unit = model.Unit?.Trim() ?? "3",
                Description = model.Description?.Trim() ?? string.Empty
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return RedirectToAction("CourseTable");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Sections)!
                    .ThenInclude(s => s.Takes)
                .Include(c => c.Sections)!
                    .ThenInclude(s => s.Teach)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
                return NotFound();

            if (course.Sections != null)
            {
                foreach (var section in course.Sections)
                {
                    if (section.Takes != null && section.Takes.Any())
                        _context.Takes.RemoveRange(section.Takes);

                    if (section.Teach != null)
                        _context.Teaches.Remove(section.Teach);
                }

                _context.Sections.RemoveRange(course.Sections);
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return RedirectToAction("CourseTable");
        }

        // ==================== CLASSROOM MANAGEMENT ====================

        public async Task<IActionResult> ClassroomTable()
        {
            var classrooms = await _context.Classrooms.ToListAsync();
            return View(classrooms);
        }

        [HttpGet]
        public IActionResult CreateClassroom()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateClassroom(Classroom classroom)
        {
            if (classroom.Capacity <= 0)
            {
                ModelState.AddModelError("Capacity", "ظرفیت کلاس باید بزرگتر از صفر باشد.");
            }

            bool duplicate = await _context.Classrooms.AnyAsync(c =>
                c.Building == classroom.Building.Trim() &&
                c.RoomNumber == classroom.RoomNumber.Trim());

            if (duplicate)
            {
                ModelState.AddModelError("RoomNumber", "کلاسی با این شماره در این ساختمان قبلاً ثبت شده است.");
            }

            if (!ModelState.IsValid)
                return View(classroom);

            classroom.Building = classroom.Building.Trim();
            classroom.RoomNumber = classroom.RoomNumber.Trim();

            _context.Classrooms.Add(classroom);
            await _context.SaveChangesAsync();

            return RedirectToAction("ClassroomTable");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteClassroom(int id)
        {
            var classroom = await _context.Classrooms
                .Include(c => c.Sections)!
                    .ThenInclude(s => s.Takes)
                .Include(c => c.Sections)!
                    .ThenInclude(s => s.Teach)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (classroom == null)
                return NotFound();

            if (classroom.Sections != null)
            {
                foreach (var section in classroom.Sections)
                {
                    if (section.Takes != null && section.Takes.Any())
                        _context.Takes.RemoveRange(section.Takes);

                    if (section.Teach != null)
                        _context.Teaches.Remove(section.Teach);
                }

                _context.Sections.RemoveRange(classroom.Sections);
            }

            _context.Classrooms.Remove(classroom);
            await _context.SaveChangesAsync();

            return RedirectToAction("ClassroomTable");
        }

        // ==================== TIMESLOT MANAGEMENT ====================

        public async Task<IActionResult> TimeSlotTable()
        {
            var slots = await _context.TimeSlots.ToListAsync();
            return View(slots);
        }

        [HttpGet]
        public IActionResult CreateTimeSlot()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTimeSlot(CreateTimeSlotViewModel model)
        {
            if (model.StartTime.TimeOfDay >= model.EndTime.TimeOfDay)
            {
                ModelState.AddModelError("EndTime", "ساعت پایان کلاس باید پس از ساعت شروع باشد.");
            }

            if (!ModelState.IsValid)
                return View(model);

            var timeSlot = new TimeSlot
            {
                Day = model.Day.Trim(),
                StartTime = model.StartTime,
                EndTime = model.EndTime
            };

            _context.TimeSlots.Add(timeSlot);
            await _context.SaveChangesAsync();

            return RedirectToAction("TimeSlotTable");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTimeSlot(int id)
        {
            var timeSlot = await _context.TimeSlots
                .Include(ts => ts.Sections)!
                    .ThenInclude(s => s.Takes)
                .Include(ts => ts.Sections)!
                    .ThenInclude(s => s.Teach)
                .FirstOrDefaultAsync(ts => ts.Id == id);

            if (timeSlot == null)
                return NotFound();

            if (timeSlot.Sections != null)
            {
                foreach (var section in timeSlot.Sections)
                {
                    if (section.Takes != null)
                        _context.Takes.RemoveRange(section.Takes);

                    if (section.Teach != null)
                        _context.Teaches.Remove(section.Teach);
                }

                _context.Sections.RemoveRange(timeSlot.Sections);
            }

            _context.TimeSlots.Remove(timeSlot);
            await _context.SaveChangesAsync();

            return RedirectToAction("TimeSlotTable");
        }

        // ==================== SECTION MANAGEMENT ====================

        public async Task<IActionResult> SectionTable()
        {
            var sections = await _context.Sections
                .Include(s => s.Course)
                .Include(s => s.Classroom)
                .Include(s => s.TimeSlot)
                .Include(s => s.Teach)!
                    .ThenInclude(t => t.Instructor)
                        .ThenInclude(i => i.User)
                .Select(s => new SectionTableViewModel
                {
                    Id = s.Id,
                    CourseTitle = s.Course.Title,
                    InstructorName = s.Teach != null && s.Teach.Instructor != null
                        ? $"{s.Teach.Instructor.User.FirstName} {s.Teach.Instructor.User.LastName}"
                        : "تعیین نشده",
                    Year = s.Year,
                    Semester = s.Semester,
                    ClassroomName = $"{s.Classroom.Building} - {s.Classroom.RoomNumber}",
                    TimeSlotInfo = $"{s.TimeSlot.Day} {s.TimeSlot.StartTime:HH\\:mm} - {s.TimeSlot.EndTime:HH\\:mm}",
                    FinalExamDate = s.FinalExamDate
                })
                .ToListAsync();

            return View(sections);
        }

        [HttpGet]
        public IActionResult CreateSection()
        {
            var model = new CreateSectionViewModel
            {
                Year = DateTime.Now.Year,
                Semester = 1,
                FinalExamDate = DateTime.Today.AddMonths(4)
            };
            FillDropDowns(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSection(CreateSectionViewModel model)
        {
            var selectedTimeSlot = await _context.TimeSlots.FirstOrDefaultAsync(ts => ts.Id == model.TimeSlotId);
            if (selectedTimeSlot == null)
            {
                ModelState.AddModelError("TimeSlotId", "بازه زمانی انتخاب‌شده نامعتبر است.");
            }

            if (ModelState.IsValid && selectedTimeSlot != null)
            {
                var selDay = selectedTimeSlot.Day;
                var selStart = selectedTimeSlot.StartTime.TimeOfDay;
                var selEnd = selectedTimeSlot.EndTime.TimeOfDay;

                // Check classroom conflict in the same year and semester with time overlap
                var allSectionsInTerm = await _context.Sections
                    .Include(s => s.TimeSlot)
                    .Where(s => s.ClassroomId == model.ClassroomId &&
                                s.Year == model.Year &&
                                s.Semester == model.Semester)
                    .ToListAsync();

                bool conflictingClassroom = allSectionsInTerm.Any(s =>
                    s.TimeSlot != null &&
                    s.TimeSlot.Day == selDay &&
                    s.TimeSlot.StartTime.TimeOfDay < selEnd &&
                    selStart < s.TimeSlot.EndTime.TimeOfDay);

                // Check instructor conflict in the same year and semester with time overlap
                var instructorTeachesInTerm = await _context.Teaches
                    .Include(t => t.Section)
                        .ThenInclude(s => s.TimeSlot)
                    .Where(t => t.InstructorId == model.InstructorId &&
                                t.Section != null &&
                                t.Section.Year == model.Year &&
                                t.Section.Semester == model.Semester)
                    .ToListAsync();

                bool conflictingInstructor = instructorTeachesInTerm.Any(t =>
                    t.Section.TimeSlot != null &&
                    t.Section.TimeSlot.Day == selDay &&
                    t.Section.TimeSlot.StartTime.TimeOfDay < selEnd &&
                    selStart < t.Section.TimeSlot.EndTime.TimeOfDay);

                if (conflictingClassroom)
                    ModelState.AddModelError("ClassroomId", "کلاس انتخاب‌شده در این بازه زمانی در همین ترم قبلاً رزرو شده است.");

                if (conflictingInstructor)
                    ModelState.AddModelError("InstructorId", "استاد انتخاب‌شده در این بازه زمانی در همین ترم کلاس دیگری دارد.");
            }

            if (!ModelState.IsValid)
            {
                FillDropDowns(model);
                return View(model);
            }

            var newSection = new Section
            {
                CourseId = model.CourseId,
                ClassroomId = model.ClassroomId,
                TimeSlotId = model.TimeSlotId,
                Year = model.Year,
                Semester = model.Semester,
                FinalExamDate = model.FinalExamDate
            };

            _context.Sections.Add(newSection);
            await _context.SaveChangesAsync();

            var teach = new Teach
            {
                SectionId = newSection.Id,
                InstructorId = model.InstructorId
            };

            _context.Teaches.Add(teach);
            await _context.SaveChangesAsync();

            return RedirectToAction("SectionTable");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SectionDelete(int id)
        {
            var section = await _context.Sections
                .Include(s => s.Takes)
                .Include(s => s.Teach)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (section == null)
                return NotFound();

            if (section.Takes != null && section.Takes.Any())
                _context.Takes.RemoveRange(section.Takes);

            if (section.Teach != null)
                _context.Teaches.Remove(section.Teach);

            _context.Sections.Remove(section);
            await _context.SaveChangesAsync();

            return RedirectToAction("SectionTable");
        }

        public async Task<IActionResult> SectionDetails(int id)
        {
            var section = await _context.Sections
                .Include(s => s.Course)
                .Include(s => s.Classroom)
                .Include(s => s.TimeSlot)
                .Include(s => s.Teach)!
                    .ThenInclude(t => t.Instructor)
                        .ThenInclude(i => i.User)
                .Include(s => s.Takes)!
                    .ThenInclude(t => t.Student)
                        .ThenInclude(st => st.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (section == null)
                return NotFound();

            var model = new SectionDetailsViewModel
            {
                Id = section.Id,
                CourseTitle = section.Course.Title,
                Year = section.Year,
                Semester = section.Semester,
                ClassroomName = $"{section.Classroom.Building} - {section.Classroom.RoomNumber}",
                TimeSlotInfo = $"{section.TimeSlot.Day} {section.TimeSlot.StartTime:HH\\:mm} - {section.TimeSlot.EndTime:HH\\:mm}",
                FinalExamDate = section.FinalExamDate,
                InstructorName = section.Teach?.Instructor != null
                    ? $"{section.Teach.Instructor.User.FirstName} {section.Teach.Instructor.User.LastName}"
                    : "تعیین نشده",
                Students = (section.Takes ?? new List<Take>()).Select(t => new StudentInSectionViewModel
                {
                    StudentId = t.StudentId,
                    StudentName = $"{t.Student.User.FirstName} {t.Student.User.LastName}",
                    Grade = t.Grade
                }).ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ChangeInstructor(int id)
        {
            var section = await _context.Sections
                .Include(s => s.Teach)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (section == null)
                return NotFound();

            var instructors = await _context.Instructors
                .Include(i => i.User)
                .Select(i => new SelectListItem
                {
                    Value = i.InstructorId.ToString(),
                    Text = $"{i.User.FirstName} {i.User.LastName}"
                }).ToListAsync();

            var model = new ChangeInstructorViewModel
            {
                SectionId = id,
                Instructors = instructors,
                SelectedInstructorId = section.Teach?.InstructorId ?? 0
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeInstructor(ChangeInstructorViewModel model)
        {
            var section = await _context.Sections
                .Include(s => s.Teach)
                .Include(s => s.TimeSlot)
                .FirstOrDefaultAsync(s => s.Id == model.SectionId);

            if (section == null)
                return NotFound();

            if (section.TimeSlot != null)
            {
                var curDay = section.TimeSlot.Day;
                var curStart = section.TimeSlot.StartTime.TimeOfDay;
                var curEnd = section.TimeSlot.EndTime.TimeOfDay;

                var otherTeachesInTerm = await _context.Teaches
                    .Include(t => t.Section)
                        .ThenInclude(s => s.TimeSlot)
                    .Where(t => t.InstructorId == model.SelectedInstructorId &&
                                t.SectionId != model.SectionId &&
                                t.Section != null &&
                                t.Section.Year == section.Year &&
                                t.Section.Semester == section.Semester)
                    .ToListAsync();

                bool instructorBusy = otherTeachesInTerm.Any(t =>
                    t.Section.TimeSlot != null &&
                    t.Section.TimeSlot.Day == curDay &&
                    t.Section.TimeSlot.StartTime.TimeOfDay < curEnd &&
                    curStart < t.Section.TimeSlot.EndTime.TimeOfDay);

                if (instructorBusy)
                {
                    ModelState.AddModelError("SelectedInstructorId", "این استاد در همین بازه زمانی در این ترم کلاس دیگری دارد.");
                }
            }

            if (!ModelState.IsValid)
            {
                model.Instructors = await _context.Instructors
                    .Include(i => i.User)
                    .Select(i => new SelectListItem
                    {
                        Value = i.InstructorId.ToString(),
                        Text = $"{i.User.FirstName} {i.User.LastName}"
                    }).ToListAsync();

                return View(model);
            }

            if (section.Teach != null)
            {
                _context.Teaches.Remove(section.Teach);
                await _context.SaveChangesAsync();
            }

            var newTeach = new Teach
            {
                SectionId = section.Id,
                InstructorId = model.SelectedInstructorId
            };

            _context.Teaches.Add(newTeach);
            await _context.SaveChangesAsync();

            return RedirectToAction("SectionDetails", new { id = model.SectionId });
        }

        [HttpGet]
        public async Task<IActionResult> AddStudentToSection(int sectionId)
        {
            var section = await _context.Sections
                .Include(s => s.Takes)
                .Include(s => s.TimeSlot)
                .FirstOrDefaultAsync(s => s.Id == sectionId);

            if (section == null)
                return NotFound();

            var currentSlot = section.TimeSlot;
            var enrolledStudentIds = section.Takes?.Select(t => t.StudentId).ToList() ?? new List<int>();

            // Find students who have an overlapping class in the SAME semester and year
            List<int> busyStudentIds = new List<int>();
            if (currentSlot != null)
            {
                var curDay = currentSlot.Day;
                var curStart = currentSlot.StartTime.TimeOfDay;
                var curEnd = currentSlot.EndTime.TimeOfDay;

                var takesInTerm = await _context.Takes
                    .Include(t => t.Section)
                        .ThenInclude(s => s.TimeSlot)
                    .Where(t => t.Section != null &&
                                t.Section.Year == section.Year &&
                                t.Section.Semester == section.Semester)
                    .ToListAsync();

                busyStudentIds = takesInTerm
                    .Where(t => t.Section.TimeSlot != null &&
                                t.Section.TimeSlot.Day == curDay &&
                                t.Section.TimeSlot.StartTime.TimeOfDay < curEnd &&
                                curStart < t.Section.TimeSlot.EndTime.TimeOfDay)
                    .Select(t => t.StudentId)
                    .Distinct()
                    .ToList();
            }

            var availableStudents = await _context.Students
                .Include(s => s.User)
                .Where(s => !enrolledStudentIds.Contains(s.StudentId) && !busyStudentIds.Contains(s.StudentId))
                .Select(s => new SelectListItem
                {
                    Value = s.StudentId.ToString(),
                    Text = $"{s.User.FirstName} {s.User.LastName} ({s.StudentId})"
                })
                .ToListAsync();

            var model = new AddStudentViewModel
            {
                SectionId = sectionId,
                Students = availableStudents
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStudentToSection(AddStudentViewModel model)
        {
            var section = await _context.Sections
                .Include(s => s.TimeSlot)
                .Include(s => s.Classroom)
                .Include(s => s.Takes)
                .FirstOrDefaultAsync(s => s.Id == model.SectionId);

            if (section == null)
                return NotFound();

            // 1. Check classroom capacity
            int currentEnrollment = section.Takes?.Count ?? 0;
            if (section.Classroom != null && currentEnrollment >= section.Classroom.Capacity)
            {
                ModelState.AddModelError(string.Empty, $"ظرفیت کلاس این گروه تکمیل شده است (ظرفیت: {section.Classroom.Capacity} نفر).");
            }

            // 2. Check duplicate enrollment
            bool alreadyExists = await _context.Takes
                .AnyAsync(t => t.SectionId == model.SectionId && t.StudentId == model.SelectedStudentId);

            if (alreadyExists)
            {
                ModelState.AddModelError(string.Empty, "این دانشجو قبلاً در این گروه درسی ثبت‌نام شده است.");
            }

            // 3. Check time overlap in the SAME year and semester
            if (section.TimeSlot != null)
            {
                var curDay = section.TimeSlot.Day;
                var curStart = section.TimeSlot.StartTime.TimeOfDay;
                var curEnd = section.TimeSlot.EndTime.TimeOfDay;

                var studentTakesInTerm = await _context.Takes
                    .Include(t => t.Section)
                        .ThenInclude(s => s.TimeSlot)
                    .Where(t => t.StudentId == model.SelectedStudentId &&
                                t.Section != null &&
                                t.Section.Year == section.Year &&
                                t.Section.Semester == section.Semester)
                    .ToListAsync();

                bool isBusy = studentTakesInTerm.Any(t =>
                    t.Section.TimeSlot != null &&
                    t.Section.TimeSlot.Day == curDay &&
                    t.Section.TimeSlot.StartTime.TimeOfDay < curEnd &&
                    curStart < t.Section.TimeSlot.EndTime.TimeOfDay);

                if (isBusy)
                {
                    ModelState.AddModelError(string.Empty, "دانشجو در این بازه زمانی در کلاس دیگری در همین ترم حضور دارد.");
                }
            }

            // 4. Check Final Exam date conflict in the SAME year and semester
            bool examConflict = await _context.Takes
                .Include(t => t.Section)
                .AnyAsync(t =>
                    t.StudentId == model.SelectedStudentId &&
                    t.SectionId != model.SectionId &&
                    t.Section.Year == section.Year &&
                    t.Section.Semester == section.Semester &&
                    t.Section.FinalExamDate.Date == section.FinalExamDate.Date);

            if (examConflict)
            {
                ModelState.AddModelError(string.Empty, "دانشجو در تاریخ امتحان نهایی این درس، آزمون درس دیگری را در برنامه خود دارد.");
            }

            if (!ModelState.IsValid)
            {
                model.Students = await _context.Students
                    .Include(s => s.User)
                    .Select(s => new SelectListItem
                    {
                        Value = s.StudentId.ToString(),
                        Text = $"{s.User.FirstName} {s.User.LastName} ({s.StudentId})"
                    })
                    .ToListAsync();

                return View(model);
            }

            var take = new Take
            {
                SectionId = model.SectionId,
                StudentId = model.SelectedStudentId,
                Grade = null // Student has no initial grade
            };

            _context.Takes.Add(take);
            await _context.SaveChangesAsync();

            return RedirectToAction("SectionDetails", new { id = model.SectionId });
        }

        [HttpGet]
        public async Task<IActionResult> UpdateGrade(int sectionId, int studentId)
        {
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
                Grade = take.Grade ?? 0,
                StudentName = $"{take.Student.User.FirstName} {take.Student.User.LastName}"
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateGrade(UpdateGradeViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var take = await _context.Takes
                .FirstOrDefaultAsync(t => t.SectionId == model.SectionId && t.StudentId == model.StudentId);

            if (take == null)
                return NotFound();

            take.Grade = model.Grade;
            await _context.SaveChangesAsync();

            return RedirectToAction("SectionDetails", new { id = model.SectionId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveStudent(int sectionId, int studentId)
        {
            var take = await _context.Takes
                .FirstOrDefaultAsync(t => t.SectionId == sectionId && t.StudentId == studentId);

            if (take == null)
                return NotFound();

            _context.Takes.Remove(take);
            await _context.SaveChangesAsync();

            return RedirectToAction("SectionDetails", new { id = sectionId });
        }
    }
}
