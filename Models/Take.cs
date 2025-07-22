namespace Golestan.Models
{
    public class Take
    {
        public int StudentId { get; set; }
        public int SectionId { get; set; }
        public int? Grade { get; set; }

        public Section Section { get; set; }
        public Student Student { get; set; }

    }

}
