using System.ComponentModel.DataAnnotations;

namespace Golestan.Models
{
    public class TimeSlot
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Day { get; set; }
        [Required]
        public DateTime StartTime { get; set; }
        [Required]
        public DateTime EndTime { get; set; }

        public ICollection<Section>? Sections { get; set; }
    }

}
