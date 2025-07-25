using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Golestan.ViewModels
{
    public class CreateSectionViewModel
    {
        [Required(ErrorMessage = "درس را انتخاب کنید.")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "کلاس را انتخاب کنید.")]
        public int ClassroomId { get; set; }

        [Required(ErrorMessage = "بازه زمانی را انتخاب کنید.")]
        public int TimeSlotId { get; set; }

        [Required(ErrorMessage = "استاد را انتخاب کنید.")]
        public int InstructorId { get; set; }

        [Required(ErrorMessage = "سال را وارد کنید.")]
        [Range(1400, 1500)]
        public int Year { get; set; }

        [Required(ErrorMessage = "ترم را وارد کنید.")]
        [Range(1, 2)]
        public int Semester { get; set; }

        [Required(ErrorMessage = "تاریخ امتحان را وارد کنید.")]
        [DataType(DataType.Date)]
        public DateTime FinalExamDate { get; set; }

        // For dropdowns (no validation needed)
        public List<SelectListItem> Courses { get; set; } = new();
        public List<SelectListItem> Classrooms { get; set; } = new();
        public List<SelectListItem> TimeSlots { get; set; } = new();
        public List<SelectListItem> Instructors { get; set; } = new();
    }
}