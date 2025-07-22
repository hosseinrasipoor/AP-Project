using System.ComponentModel.DataAnnotations;

namespace Golestan.Models
{
    public class Student
    {
        [Key]
        public int StudentId { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public DateTime EnrollmentDate { get; set; }

        
        public ICollection<Take>? Takes { get; set; }
    }

}
