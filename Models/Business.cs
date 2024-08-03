
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
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public Address Address { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber {  get; set; }

        [Required]
        public ICollection<WorkingHours> WorkingHours { get; set; } = new List<WorkingHours>();
    }

    public class Address
    {
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string BuildingNumber { get; set; }
        public string RoomNumber { get; set; }
    }

    public class WorkingHours
    {
        public int BusinessId {  get; set; }
        public DayOfWeek Day { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}
