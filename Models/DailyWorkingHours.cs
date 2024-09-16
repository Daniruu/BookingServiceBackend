using System.ComponentModel.DataAnnotations;

namespace BookingServiceBackend.Models
{
    public class DailyWorkingHours
    {
        [Key]
        public int Id { get; set; }
        public int BusinessId { get; set; }
        public DayOfWeek Day { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }

        public Business Business { get; set; }
    }
}
