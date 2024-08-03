using System.ComponentModel.DataAnnotations;

namespace BookingServiceBackend.Models
{
    public class Service
    {
        [Key]
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int Duration {  get; set; }
    }
}
