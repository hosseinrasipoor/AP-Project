using System.ComponentModel.DataAnnotations;

namespace Golestan.ViewModels
{
    public class CreateUserViewModel
    {
        [Display(Name = "نام")]
        [Required(ErrorMessage = "وارد کردن نام الزامی است.")]
        [StringLength(50, ErrorMessage = "نام نباید بیشتر از ۵۰ کاراکتر باشد.")]
        public string FirstName { get; set; }

        [Display(Name = "نام خانوادگی")]
        [Required(ErrorMessage = "وارد کردن نام خانوادگی الزامی است.")]
        [StringLength(50, ErrorMessage = "نام خانوادگی نباید بیشتر از ۵۰ کاراکتر باشد.")]
        public string LastName { get; set; }

        [Display(Name = "پست الکترونیکی")]
        [Required(ErrorMessage = "وارد کردن ایمیل الزامی است.")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل وارد شده معتبر نیست.")]
        public string Email { get; set; }

        [Display(Name = "کلمه عبور")]
        [Required(ErrorMessage = "وارد کردن کلمه عبور الزامی است.")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "کلمه عبور باید حداقل ۶ کاراکتر باشد.")]
        public string Password { get; set; }
    }
}
