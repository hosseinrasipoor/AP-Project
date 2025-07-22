namespace Golestan.Models
{
    public class Section
    {
        public int Id { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }

        public int Year { get; set; }
        public int Semester { get; set; }

        public int ClassroomId { get; set; }
        public Classroom Classroom { get; set; }

        public int TimeSlotId { get; set; }
        public TimeSlot TimeSlot { get; set; }

        public DateTime FinalExamDate { get; set; }

        
        
        
        public ICollection<Take>? Takes { get; set; }
        public Teach Teach { get; set; }
    }

}
