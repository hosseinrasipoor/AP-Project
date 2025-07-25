using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Golestan.ViewModels
{
    public class UpdateGradeViewModel
    {
        public int SectionId { get; set; }
        public int StudentId { get; set; }

        [Required(ErrorMessage = "وارد کردن نمره الزامی است")]
        [Range(0, 20, ErrorMessage = "نمره باید بین 0 تا 20 باشد")]
        public int Grade { get; set; }
        [ValidateNever]
        public string StudentName { get; set; }
    }





}
