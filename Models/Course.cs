
using System.ComponentModel.DataAnnotations;

namespace Golestan.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Title { get; set; }

        [Required, MaxLength(50)]
        public string Code { get; set; }
        [MaxLength(50)]
        public string Unit { get; set; }
        [MaxLength(50)]
        public string Description { get; set; }

        public DateTime ExamDate { get; set; }

        public ICollection<Section> Sections { get; set; }
    }

}
