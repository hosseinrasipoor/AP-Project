using System.ComponentModel.DataAnnotations;

namespace Golestan.ViewModels
{
    public class CreateInstructorViewModel
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Salary must be a positive number.")]
        public decimal Salary { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime HireDate { get; set; }
    }

}
