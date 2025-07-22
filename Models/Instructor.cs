namespace Golestan.Models
{
    public class Instructor
    {
        public int InstructorId { get; set; }

        public int UserId { get; set; }
        

        public decimal Salary { get; set; }
        public DateTime HireDate { get; set; }

        public User User { get; set; }
        public ICollection<Teaches> Teaches { get; set; }
    }

}
