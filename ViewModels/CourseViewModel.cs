using System.ComponentModel.DataAnnotations;

namespace Golestan.ViewModels
{
    public class CreateCourseViewModel
    {
        [Display(Name = "عنوان درس")]
        [Required(ErrorMessage = "وارد کردن عنوان درس الزامی است.")]
        [MaxLength(50, ErrorMessage = "عنوان درس نباید بیشتر از ۵۰ کاراکتر باشد.")]
        public string Title { get; set; }

        [Display(Name = "کد درس")]
        [Required(ErrorMessage = "وارد کردن کد درس الزامی است.")]
        [MaxLength(50, ErrorMessage = "کد درس نباید بیشتر از ۵۰ کاراکتر باشد.")]
        public string Code { get; set; }

        [Display(Name = "تعداد واحد")]
        [Required(ErrorMessage = "وارد کردن تعداد واحد الزامی است.")]
        [MaxLength(50, ErrorMessage = "تعداد واحد نباید بیشتر از ۵۰ کاراکتر باشد.")]
        public string Unit { get; set; }

        [Display(Name = "توضیحات")]
        [MaxLength(200, ErrorMessage = "توضیحات درس نباید بیشتر از ۲۰۰ کاراکتر باشد.")]
        public string? Description { get; set; }
    }
}
