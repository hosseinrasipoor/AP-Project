using System.ComponentModel.DataAnnotations;

namespace Golestan.Models
{
    public class Classroom
    {
        public int Id { get; set; }
        [MaxLength(255)]
        public string Building { get; set; }
        public string RoomNumber { get; set; }
        public int Capacity { get; set; }

        public ICollection<Section>? Sections { get; set; }
    }

}
