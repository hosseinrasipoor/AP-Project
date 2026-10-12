using System.ComponentModel.DataAnnotations;

namespace Golestan.ViewModels
{
    public class CreateStudentViewModel
    {
        [Display(Name = "کاربر")]
        [Required(ErrorMessage = "انتخاب کاربر الزامی است.")]
        public int UserId { get; set; }

        [Display(Name = "تاریخ پذیرش")]
        [Required(ErrorMessage = "وارد کردن تاریخ پذیرش الزامی است.")]
        [DataType(DataType.Date)]
        public DateTime EnrollmentDate { get; set; }
    }
}
