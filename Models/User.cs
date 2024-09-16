using System.ComponentModel.DataAnnotations;

namespace BookingServiceBackend.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string SecondName { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        public string? RefreshToken {  get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}
