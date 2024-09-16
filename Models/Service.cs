using System.ComponentModel.DataAnnotations;

namespace BookingServiceBackend.Models
{
    public class Service
    {
        [Key]
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public int BusinessId {  get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int Duration {  get; set; }

        public bool IsFeatured { get; set; }

        public string Group { get; set; }

        public Business Business { get; set; }

        public Employee Employee { get; set; }
    }
}
