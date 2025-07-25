using System.ComponentModel.DataAnnotations;

namespace Golestan.ViewModels.Account
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "وارد کردن ایمیل الزامی است.")]
        [EmailAddress(ErrorMessage = "ایمیل وارد شده معتبر نیست.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "وارد کردن رمز عبور الزامی است.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

}
