
using System.ComponentModel.DataAnnotations;

namespace BookingServiceBackend.Models
{
    public class Business
    {
        [Key]
        public int Id { get; set; }
        
        public int UserId {  get; set; }

        [Required]
        public string Category {  get; set; }

        [Required]
        public string Nip { get; set; }

        [Required]
        public string Regon {  get; set; }

        public string Krs { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        public Address Address { get; set; }

        public ICollection<DailyWorkingHours> WeeklyWorkingHours { get; set; }

        public ICollection<Employee> Employees { get; set; }

        public ICollection<Service> Services { get; set; }
    }
}
