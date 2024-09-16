using System.ComponentModel.DataAnnotations;

namespace BookingServiceBackend.Models
{
    public class Reservation
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        public int ServiceId { get; set; }

        [Required]
        public DateTime DateTime { get; set; }

        [Required]
        public string Status { get; set; }

        public Service Service { get; set; }
    }
}
