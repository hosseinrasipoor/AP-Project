using System.ComponentModel.DataAnnotations;

namespace Golestan.ViewModels
{
    public class CreateInstructorViewModel
    {
        [Display(Name = "کاربر")]
        [Required(ErrorMessage = "انتخاب کاربر الزامی است.")]
        public int UserId { get; set; }

        [Display(Name = "حقوق")]
        [Required(ErrorMessage = "وارد کردن مبلغ حقوق الزامی است.")]
        [Range(0, double.MaxValue, ErrorMessage = "حقوق باید عددی مثبت باشد.")]
        public decimal Salary { get; set; }

        [Display(Name = "تاریخ استخدام")]
        [Required(ErrorMessage = "وارد کردن تاریخ استخدام الزامی است.")]
        [DataType(DataType.Date)]
        public DateTime HireDate { get; set; }
    }
}
