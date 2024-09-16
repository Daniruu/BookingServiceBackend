using BookingServiceBackend.Data;
using BookingServiceBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NodaTime.Text;
using NodaTime.TimeZones;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace BookingServiceBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class ReservationController : ControllerBase
    {
        private readonly BookignServiceDbContext _context;

        public ReservationController(BookignServiceDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateReservation([FromBody] ReservationDto request)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("You don't have access");
            }

            var userId = int.Parse(userIdClaim);

            var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == request.ServiceId);
            if (service == null)
            {
                return BadRequest("Service not found");
            }

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == service.EmployeeId);
            if (employee == null)
            {
                return BadRequest("Employee not found");
            }

            var conflictingReservation = await _context.Reservations
                .Include(r => r.Service)
                .Where(r => r.Service.EmployeeId == employee.Id)
                .FirstOrDefaultAsync(r =>
                    (r.DateTime <= request.DateTime && request.DateTime < r.DateTime.AddMinutes(r.Service.Duration)) ||
                    (request.DateTime <= r.DateTime && r.DateTime < request.DateTime.AddMinutes(service.Duration)));

            if (conflictingReservation != null)
            {
                return BadRequest("That time has already been reserved");
            }

            var reservation = new Reservation
            {
                UserId = userId,
                ServiceId = request.ServiceId,
                DateTime = request.DateTime.ToUniversalTime(),
                Status = request.Status ?? "Pending"
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Reservation added successfully" });
        }


        [HttpGet]
        public async Task<IActionResult> GetUserReservations()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("You don't have access");
            }

            var userId = int.Parse(userIdClaim);

            var reservations = await _context.Reservations
                .Include(r => r.Service)
                    .ThenInclude(s => s.Employee)
                .Where(r => r.UserId == userId)
                .ToListAsync();

            if (reservations == null || reservations.Count == 0)
            {
                return NotFound("Reservations not found");
            }

            return Ok(reservations);
        }

        [HttpGet("{businessId}")]
        public async Task<IActionResult> GetBusinessReservations(int businessId, [FromQuery] DateTime date)
        {
            Console.WriteLine($"Received businessId: {businessId}, date: {date}");

            date = DateTime.SpecifyKind(date, DateTimeKind.Utc);

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("You don't have access");
            }

            var userId = int.Parse(userIdClaim);

            var reservations = await _context.Reservations
                .Include(r => r.Service)
                    .ThenInclude(s => s.Employee)
                .Where(r => r.Service.BusinessId == businessId && r.DateTime.Date == date.Date)
                .ToListAsync();

            if (reservations == null || reservations.Count == 0)
            {
                return NotFound("Reservations not found");
            }

            var groupedReservation = reservations.GroupBy(r => r.Service.Employee);

            var result = groupedReservation.Select(group => new
            {
                Employee = new
                {
                    group.Key.Id,
                    group.Key.Name,
                },
                Reservations = group.Select(r => new
                {
                    r.Id,
                    r.DateTime,
                    r.Status,
                    Service = new
                    {
                        r.Service.Id,
                        r.Service.Name,
                        r.Service.Description,
                        r.Service.Price,
                        r.Service.Duration,
                        r.Service.Group
                    }
                }).ToList()
            }).ToList();

            return Ok(result);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> DeleteReservaton(int id)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized("You don't have access");
                }
                var userId = int.Parse(userIdClaim);

                var business = await _context.Businesses.FirstOrDefaultAsync(b => b.UserId == userId);

                if (business == null)
                {
                    return BadRequest("Business not found.");
                }

                var reservation = await _context.Reservations.FirstOrDefaultAsync(r => r.Id == id);

                if (reservation == null)
                {
                    return NotFound("Reservation not found");
                }

                if (reservation.UserId != userId)
                {
                    return Unauthorized("You don't have access");
                }

                reservation.Status = "cancelled";

                _context.Reservations.Update(reservation);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Reservation deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpGet("available-timeslots")]
        public async Task<IActionResult> GetAvailableTimeSlots([FromQuery] int serviceId, [FromQuery] DateTime date, [FromQuery] string timeZone)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("You don't have access");
            }

            date = DateTime.SpecifyKind(date, DateTimeKind.Utc);

            var service = await _context.Services.FindAsync(serviceId);
            if (service == null)
            {
                return NotFound("Service not found");
            }

            var employee = await _context.Employees.FindAsync(service.EmployeeId);
            if (employee == null)
            {
                return NotFound("Employee not found");
            }

            var business = await _context.Businesses
                .Include(b => b.WeeklyWorkingHours)
                .FirstOrDefaultAsync(b => b.Id == employee.BusinessId);

            if (business == null)
            {
                return NotFound("Business not found");
            }

            DateTimeZone clientTimeZone;
            try
            {
                clientTimeZone = DateTimeZoneProviders.Tzdb[timeZone];
            }
            catch (DateTimeZoneNotFoundException)
            {
                return BadRequest("Invalid time zone");
            }

            var workingHoursForDay = business.WeeklyWorkingHours
                .FirstOrDefault(wh => wh.Day == date.DayOfWeek);

            if (workingHoursForDay == null)
            {
                return BadRequest("Business is closed on this day");
            }

            var startTime = TimeSpan.Parse(workingHoursForDay.StartTime);
            var endTime = TimeSpan.Parse(workingHoursForDay.EndTime);

            var reservations = await _context.Reservations
                .Include(r => r.Service)
                .Where(r => r.Service.EmployeeId == employee.Id && r.DateTime.Date == date.Date && r.Status == "active")
                .ToListAsync();

            var availableSlots = new List<DateTime>();

            for (var time = startTime; time < endTime; time = time.Add(TimeSpan.FromMinutes(15)))
            {
                var slotStart = date.Date + time;
                var slotEnd = slotStart.AddMinutes(service.Duration);

                if (!reservations.Any(r =>
                    (r.DateTime <= slotStart && slotStart < r.DateTime.AddMinutes(r.Service.Duration)) ||
                    (slotStart <= r.DateTime && r.DateTime < slotEnd)) &&
                    slotEnd.TimeOfDay <= endTime)
                {
                    availableSlots.Add(ConvertUtcToLocalDateTime(slotStart, clientTimeZone));
                }
            }

            return Ok(availableSlots.Select(slot => slot.ToString("yyyy-MM-ddTHH:mm:ss")));
        }

        private DateTime ConvertUtcToLocalDateTime(DateTime utcDateTime, DateTimeZone clientTimeZone)
        {
            var instant = Instant.FromDateTimeUtc(utcDateTime);
            var clientZonedDateTime = instant.InZone(clientTimeZone);

            return clientZonedDateTime.ToDateTimeUnspecified();
        }

    }

    public class ReservationDto
    {
        [Required]
        public int ServiceId { get; set; }

        [Required]
        public DateTime DateTime { get; set; }

        public string Status { get; set; } = "Active";
    }
}