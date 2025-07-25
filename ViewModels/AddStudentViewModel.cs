using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Golestan.ViewModels
{
    public class AddStudentViewModel
    {
        public int SectionId { get; set; }

        [Required(ErrorMessage = "لطفاً یک دانشجو انتخاب کنید.")]
        public int SelectedStudentId { get; set; }

        public List<SelectListItem> Students { get; set; } = new List<SelectListItem>();
    }

}
