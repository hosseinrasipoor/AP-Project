namespace Golestan.Models
{
    public class Student
    {
        public int StudentId { get; set; }

        public int UserId { get; set; }
        

        public DateTime EnrollmentDate { get; set; }

        public User User { get; set; }
        public ICollection<Take> Takes { get; set; }
    }

}
