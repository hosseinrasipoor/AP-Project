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
                Text = c.Title
            }).ToList();

            model.Classrooms = _context.Classrooms.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Building + " " + c.RoomNumber
            }).ToList();

            model.TimeSlots = _context.TimeSlots.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = t.Day + " " + t.StartTime.ToString("HH:mm") + "-" + t.EndTime.ToString("HH:mm")
            }).ToList();

            model.Instructors = _context.Instructors.Include(i => i.User).Select(i => new SelectListItem
            {
                Value = i.InstructorId.ToString(),
                Text = i.User.FirstName + " " + i.User.LastName
            }).ToList();
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
        public async Task<IActionResult> CreateStudent(CreateStudentViewModel model)
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
            await AssignRoleToUserAsync(model.UserId, RoleType.Student);
            _context.Students.Add(student);
            await _context.SaveChangesAsync();



            return RedirectToAction("StudentTable");
        }

        
        [HttpPost]
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
                
                var studentRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == RoleType.Student);
                if (studentRole != null)
                {
                    var userRole = await _context.UserRoles
                        .FirstOrDefaultAsync(ur => ur.UserId == student.UserId && ur.RoleId == studentRole.Id);

                    if (userRole != null)
                    {
                        _context.UserRoles.Remove(userRole);
                        await _context.SaveChangesAsync();
                    }
                }
            }

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
        public async Task<IActionResult> CreateInstructor(CreateInstructorViewModel model)
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
            await AssignRoleToUserAsync(model.UserId, RoleType.Instructor);
            _context.Instructors.Add(instructor);
            await _context.SaveChangesAsync();

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
                
                var instructorRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == RoleType.Instructor);
                if (instructorRole != null)
                {
                    var userRole = await _context.UserRoles
                        .FirstOrDefaultAsync(ur => ur.UserId == instructor.UserId && ur.RoleId == instructorRole.Id);

                    if (userRole != null)
                    {
                        _context.UserRoles.Remove(userRole);
                        await _context.SaveChangesAsync();
                    }
                }
            }

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

        public IActionResult ClassroomTable()
        {
            var classrooms = _context.Classrooms.ToList();
            return View(classrooms);
        }

        [HttpGet]
        public IActionResult CreateClassroom()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateClassroom(Classroom classroom)
        {
            if (!ModelState.IsValid)
                return View(classroom);

            _context.Classrooms.Add(classroom);
            _context.SaveChanges();

            return RedirectToAction("ClassroomTable");
        }


        [HttpPost]
        public async Task<IActionResult> DeleteClassroom(int id)
        {
            var classroom = await _context.Classrooms
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Takes)
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Teach)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (classroom == null)
                return NotFound();

            foreach (var section in classroom.Sections)
            {

                if (section.Takes != null && section.Takes.Any())
                    _context.Takes.RemoveRange(section.Takes);


                if (section.Teach != null)
                    _context.Teaches.Remove(section.Teach);
            }


            _context.Sections.RemoveRange(classroom.Sections);


            _context.Classrooms.Remove(classroom);

            await _context.SaveChangesAsync();

            return RedirectToAction("ClassroomTable");
        }

        public IActionResult TimeSlotTable()
        {
            var timeSlots = _context.TimeSlots.ToList();
            return View(timeSlots);
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
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var timeSlot = new TimeSlot
            {
                Day = model.Day,
                StartTime = model.StartTime,
                EndTime = model.EndTime
            };

            _context.TimeSlots.Add(timeSlot);
            await _context.SaveChangesAsync();

            return RedirectToAction("TimeSlotTable");
        }

        public async Task<IActionResult> DeleteTimeSlot(int id)
        {
            var timeSlot = await _context.TimeSlots
                .Include(ts => ts.Sections)
                    .ThenInclude(s => s.Takes)
                .Include(ts => ts.Sections)
                    .ThenInclude(s => s.Teach)
                .FirstOrDefaultAsync(ts => ts.Id == id);

            if (timeSlot == null)
                return NotFound();

            foreach (var section in timeSlot.Sections)
            {
                if (section.Takes != null)
                    _context.Takes.RemoveRange(section.Takes);

                if (section.Teach != null)
                    _context.Teaches.Remove(section.Teach);
            }

            _context.Sections.RemoveRange(timeSlot.Sections);
            _context.TimeSlots.Remove(timeSlot);

            await _context.SaveChangesAsync();
            return RedirectToAction("TimeSlotTable");
        }







        [HttpGet]
        public IActionResult CreateSection()
        {
            var model = new CreateSectionViewModel
            {
                Courses = _context.Courses
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Title
                    }).ToList(),

                Classrooms = _context.Classrooms
                    .Select(cl => new SelectListItem
                    {
                        Value = cl.Id.ToString(),
                        Text = $"{cl.Building} - {cl.RoomNumber}"
                    }).ToList(),

                TimeSlots = _context.TimeSlots
                    .Select(ts => new SelectListItem
                    {
                        Value = ts.Id.ToString(),
                        Text = $"{ts.Day} ({ts.StartTime:hh\\:mm} - {ts.EndTime:hh\\:mm})"
                    }).ToList(),

                Instructors = _context.Instructors
                    .Include(i => i.User)
                    .Select(i => new SelectListItem
                    {
                        Value = i.InstructorId.ToString(),
                        Text = $"{i.User.FirstName} {i.User.LastName}"
                    }).ToList()
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateSection(CreateSectionViewModel model)
        {
            
            if (!ModelState.IsValid)
            {
                model.Courses = _context.Courses
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Title
                    }).ToList();

                model.Classrooms = _context.Classrooms
                    .Select(cl => new SelectListItem
                    {
                        Value = cl.Id.ToString(),
                        Text = $"{cl.Building} - {cl.RoomNumber}"
                    }).ToList();

                model.TimeSlots = _context.TimeSlots
                    .Select(ts => new SelectListItem
                    {
                        Value = ts.Id.ToString(),
                        Text = $"{ts.Day} ({ts.StartTime:hh\\:mm} - {ts.EndTime:hh\\:mm})"
                    }).ToList();

                model.Instructors = _context.Instructors
                    .Include(i => i.User)
                    .Select(i => new SelectListItem
                    {
                        Value = i.InstructorId.ToString(),
                        Text = $"{i.User.FirstName} {i.User.LastName}"
                    }).ToList();

                return View(model);
            }

           
            var conflictingClassroom = _context.Sections
                .Any(s => s.ClassroomId == model.ClassroomId && s.TimeSlotId == model.TimeSlotId);

            var conflictingInstructor = _context.Teaches
                .Include(t => t.Section)
                .Any(t => t.InstructorId == model.InstructorId && t.Section.TimeSlotId == model.TimeSlotId);

            if (conflictingClassroom)
                ModelState.AddModelError("ClassroomId", "کلاس انتخاب‌شده در این بازه زمانی قبلاً استفاده شده است.");

            if (conflictingInstructor)
                ModelState.AddModelError("InstructorId", "استاد انتخاب‌شده در این بازه زمانی قبلاً مشغول تدریس است.");

            if (!ModelState.IsValid)
            {
                
                model.Courses = _context.Courses
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Title
                    }).ToList();

                model.Classrooms = _context.Classrooms
                    .Select(cl => new SelectListItem
                    {
                        Value = cl.Id.ToString(),
                        Text = $"{cl.Building} - {cl.RoomNumber}"
                    }).ToList();

                model.TimeSlots = _context.TimeSlots
                    .Select(ts => new SelectListItem
                    {
                        Value = ts.Id.ToString(),
                        Text = $"{ts.Day} ({ts.StartTime:hh\\:mm} - {ts.EndTime:hh\\:mm})"
                    }).ToList();

                model.Instructors = _context.Instructors
                    .Include(i => i.User)
                    .Select(i => new SelectListItem
                    {
                        Value = i.InstructorId.ToString(),
                        Text = $"{i.User.FirstName} {i.User.LastName}"
                    }).ToList();

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
            _context.SaveChanges();

            
            var teach = new Teach
            {
                SectionId = newSection.Id,
                InstructorId = model.InstructorId
            };

            _context.Teaches.Add(teach);
            _context.SaveChanges();

            return RedirectToAction("SectionTable");
        }





        public IActionResult SectionTable()
        {
            var sections = _context.Sections
                .Include(s => s.Course)
                .Include(s => s.Classroom)
                .Include(s => s.TimeSlot)
                .Include(s => s.Teach)
                    .ThenInclude(t => t.Instructor)
                        .ThenInclude(i => i.User)
                .Select(s => new SectionTableViewModel
                {
                    Id = s.Id,
                    CourseTitle = s.Course.Title,
                    InstructorName = s.Teach != null
                        ? $"{s.Teach.Instructor.User.FirstName} {s.Teach.Instructor.User.LastName}"
                        : "نامشخص",
                    Year = s.Year,
                    Semester = s.Semester,
                    ClassroomName = $"{s.Classroom.Building} - {s.Classroom.RoomNumber}",
                    TimeSlotInfo = $"{s.TimeSlot.Day} {s.TimeSlot.StartTime:hh\\:mm} - {s.TimeSlot.EndTime:hh\\:mm}",
                    FinalExamDate = s.FinalExamDate
                })
                .ToList();

            return View(sections);
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
                .Include(s => s.Teach)
                    .ThenInclude(t => t.Instructor)
                        .ThenInclude(i => i.User)
                .Include(s => s.Takes)
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
                ClassroomName = section.Classroom.Building + " " + section.Classroom,
                TimeSlotInfo = $"{section.TimeSlot.Day} {section.TimeSlot.StartTime} - {section.TimeSlot.EndTime}",
                FinalExamDate = section.FinalExamDate,
                InstructorName = section.Teach?.Instructor != null
                     ? section.Teach.Instructor.User.FirstName + " " + section.Teach.Instructor.User.LastName
                     : null,
                Students = section.Takes.Select(t => new StudentInSectionViewModel
                {
                    StudentId = t.StudentId,
                    StudentName = t.Student.User.FirstName + " " + t.Student.User.LastName,
                    Grade = t.Grade
                }).ToList()
            };


            return View(model);
        }

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
                    Text = i.User.FirstName + i.User.LastName
                })
                .ToListAsync();

            var model = new ChangeInstructorViewModel
            {
                SectionId = id,
                Instructors = instructors,
                SelectedInstructorId = section.Teach?.InstructorId ?? 0
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeInstructor(ChangeInstructorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState Invalid");

                model.Instructors = await _context.Instructors
                    .Include(i => i.User)
                    .Select(i => new SelectListItem
                    {
                        Value = i.InstructorId.ToString(),
                        Text = i.User.FirstName + " " + i.User.LastName
                    })
                    .ToListAsync();

                return View(model);
            }

            var section = await _context.Sections
                .Include(s => s.Teach)
                .FirstOrDefaultAsync(s => s.Id == model.SectionId);

            if (section == null)
                return NotFound();

            var timeSlotId = section.TimeSlotId;

            bool instructorBusy = await _context.Teaches
                .Include(t => t.Section)
                .AnyAsync(t =>
                    t.InstructorId == model.SelectedInstructorId &&
                    t.Section.TimeSlotId == timeSlotId &&
                    t.SectionId != model.SectionId);

            if (instructorBusy)
            {
                ModelState.AddModelError("SelectedInstructorId", "این استاد در این بازه زمانی سشن دیگری دارد.");

                model.Instructors = await _context.Instructors
                    .Include(i => i.User)
                    .Select(i => new SelectListItem
                    {
                        Value = i.InstructorId.ToString(),
                        Text = i.User.FirstName + " " + i.User.LastName
                    })
                    .ToListAsync();

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
                .Include(s => s.TimeSlot)
                .FirstOrDefaultAsync(s => s.Id == sectionId);

            if (section == null)
                return NotFound();

            int timeSlotId = section.TimeSlotId;

            
            var busyStudentIds = await _context.Takes
                .Where(t => t.Section.TimeSlotId == timeSlotId)
                .Select(t => t.StudentId)
                .ToListAsync();

            var availableStudents = await _context.Students
                .Include(s => s.User)
                .Where(s => !busyStudentIds.Contains(s.StudentId))
                .Select(s => new SelectListItem
                {
                    Value = s.StudentId.ToString(),
                    Text = s.User.FirstName + " " + s.User.LastName
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
        public async Task<IActionResult> AddStudentToSection(AddStudentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Students = await _context.Students
                    .Include(s => s.User)
                    .Select(s => new SelectListItem
                    {
                        Value = s.StudentId.ToString(),
                        Text = s.User.FirstName + " " + s.User.LastName
                    })
                    .ToListAsync();

                return View(model);
            }

            var section = await _context.Sections
                .Include(s => s.TimeSlot)
                .FirstOrDefaultAsync(s => s.Id == model.SectionId);

            if (section == null)
                return NotFound();

            int timeSlotId = section.TimeSlotId;

            
            bool isBusy = await _context.Takes
                .AnyAsync(t => t.StudentId == model.SelectedStudentId && t.Section.TimeSlotId == timeSlotId);

            if (isBusy)
            {
                ModelState.AddModelError("", "دانشجو در این بازه زمانی در کلاس دیگری حضور دارد.");
                model.Students = await _context.Students
                    .Include(s => s.User)
                    .Select(s => new SelectListItem
                    {
                        Value = s.StudentId.ToString(),
                        Text = s.User.FirstName + " " + s.User.LastName
                    })
                    .ToListAsync();

                return View(model);
            }

            
            bool alreadyExists = await _context.Takes
                .AnyAsync(t => t.SectionId == model.SectionId && t.StudentId == model.SelectedStudentId);

            if (alreadyExists)
            {
                ModelState.AddModelError("", "این دانشجو قبلاً در این سشن ثبت شده است.");
                return View(model);
            }

            var take = new Take
            {
                SectionId = model.SectionId,
                StudentId = model.SelectedStudentId,
                Grade = 0
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
                Grade = take.Grade,
                StudentName = take.Student.User.FirstName + " " + take.Student.User.LastName
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateGrade(UpdateGradeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine("Validation Error: " + error.ErrorMessage);
                }
                
                var student = await _context.Students
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.StudentId == model.StudentId);

                model.StudentName = student != null ? student.User.FirstName + " " + student.User.LastName : "";

                return View(model);
            }

            var take = await _context.Takes
                .FirstOrDefaultAsync(t => t.SectionId == model.SectionId && t.StudentId == model.StudentId);

            if (take == null)
                return NotFound();

            take.Grade = model.Grade;
            await _context.SaveChangesAsync();

            return RedirectToAction("SectionDetails", new { id = model.SectionId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveStudent(int sectionId, int studentId)
        {
            
            var take = await _context.Takes
                .FirstOrDefaultAsync(t => t.SectionId == sectionId && t.StudentId == studentId);

            if (take == null)
            {
                return NotFound();
            }

            _context.Takes.Remove(take);
            await _context.SaveChangesAsync();

            return RedirectToAction("SectionDetails", new { id = sectionId });
        }



    }
}
