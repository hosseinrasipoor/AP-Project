using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Golestan.ViewModels
{
    public class CreateSectionViewModel
    {
        [Display(Name = "درس")]
        [Required(ErrorMessage = "انتخاب درس الزامی است.")]
        public int CourseId { get; set; }

        [Display(Name = "کلاس")]
        [Required(ErrorMessage = "انتخاب کلاس الزامی است.")]
        public int ClassroomId { get; set; }

        [Display(Name = "بازه زمانی")]
        [Required(ErrorMessage = "انتخاب بازه زمانی الزامی است.")]
        public int TimeSlotId { get; set; }

        [Display(Name = "استاد")]
        [Required(ErrorMessage = "انتخاب استاد الزامی است.")]
        public int InstructorId { get; set; }

        [Display(Name = "سال تحصیلی")]
        [Required(ErrorMessage = "وارد کردن سال الزامی است.")]
        [Range(1400, 1500, ErrorMessage = "سال تحصیلی باید بین ۱۴۰۰ تا ۱۵۰۰ باشد.")]
        public int Year { get; set; }

        [Display(Name = "ترم")]
        [Required(ErrorMessage = "وارد کردن ترم الزامی است.")]
        [Range(1, 2, ErrorMessage = "ترم باید ۱ یا ۲ باشد.")]
        public int Semester { get; set; }

        [Display(Name = "تاریخ امتحان نهایی")]
        [Required(ErrorMessage = "وارد کردن تاریخ امتحان الزامی است.")]
        [DataType(DataType.Date)]
        public DateTime FinalExamDate { get; set; }

        // For dropdowns (no validation needed)
        public List<SelectListItem> Courses { get; set; } = new();
        public List<SelectListItem> Classrooms { get; set; } = new();
        public List<SelectListItem> TimeSlots { get; set; } = new();
        public List<SelectListItem> Instructors { get; set; } = new();
    }
}