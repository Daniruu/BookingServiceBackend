using System.ComponentModel.DataAnnotations;

namespace BookingServiceBackend.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }
        public int BusinessId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Position { get; set; }
    }
}
