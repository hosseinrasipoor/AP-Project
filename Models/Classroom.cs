using System.ComponentModel.DataAnnotations;

namespace Golestan.Models
{
    public class Classroom
    {
        public int Id { get; set; }

        [Display(Name = "نام ساختمان / دانشکده")]
        [Required(ErrorMessage = "وارد کردن نام ساختمان الزامی است.")]
        [MaxLength(255, ErrorMessage = "نام ساختمان نباید بیشتر از ۲۵۵ کاراکتر باشد.")]
        public string Building { get; set; }

        [Display(Name = "شماره کلاس")]
        [Required(ErrorMessage = "وارد کردن شماره کلاس الزامی است.")]
        public string RoomNumber { get; set; }

        [Display(Name = "ظرفیت")]
        [Required(ErrorMessage = "وارد کردن ظرفیت الزامی است.")]
        [Range(1, 500, ErrorMessage = "ظرفیت باید حداقل ۱ نفر باشد.")]
        public int Capacity { get; set; }

        public ICollection<Section>? Sections { get; set; }
    }
}
