using System.ComponentModel.DataAnnotations;
namespace Golestan.ViewModels
{
    public class CreateStudentViewModel
    {
        [Required(ErrorMessage = "User is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Enrollment date is required")]
        [DataType(DataType.Date)]
        public DateTime EnrollmentDate { get; set; }
    }

}
