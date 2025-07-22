using System.ComponentModel.DataAnnotations;

namespace Golestan.Models
{
    public class Take
    {
        public int StudentId { get; set; }
        public Student Student { get; set; }

        public int SectionId { get; set; }
        public Section Section { get; set; }

        [Range(0,20)]
        public int Grade { get; set;}

        
        

    }

}
