using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Golestan.ViewModels
{
    public class ChangeInstructorViewModel
    {
        public int SectionId { get; set; }

        [Required(ErrorMessage = "لطفاً یک استاد انتخاب کنید.")]
        public int SelectedInstructorId { get; set; }

        public List<SelectListItem> Instructors { get; set; } = new List<SelectListItem>();
    }

}

