namespace Golestan.Models
{
    public class Teach
    {
        public int InstructorId { get; set; }
        public int SectionId { get; set; }


        public Section Section { get; set; }
        public Instructor Instructor { get; set; }
    }

}
