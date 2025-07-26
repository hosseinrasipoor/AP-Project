using System.ComponentModel.DataAnnotations;

namespace Golestan.ViewModels
{
    public class CreateCourseViewModel
    {
        [Required, MaxLength(50)]
        public string Title { get; set; }

        [Required, MaxLength(50)]
        public string Code { get; set; }

        [MaxLength(50)]
        public string Unit { get; set; }

        [MaxLength(50)]
        public string Description { get; set; }

       
        
    }

}
