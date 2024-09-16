using BookingServiceBackend.Data;
using BookingServiceBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static BookingServiceBackend.Controllers.BusinessController;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using NodaTime;
using NodaTime.Text;
using NodaTime.TimeZones;

namespace BookingServiceBackend.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class WorkingHoursController : ControllerBase
    {

        private readonly BookignServiceDbContext _context;

        public WorkingHoursController(BookignServiceDbContext context)
        {
            _context = context;
        }

        [HttpPost("{businessId}")]
        [Authorize]
        public async Task<IActionResult> AddWorkingHours(int businessId, [FromBody] DailyWorkingHoursDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var business = await _context.Businesses
                    .Include(b => b.WeeklyWorkingHours)
                    .FirstOrDefaultAsync(b => b.Id == businessId);

                if (business == null)
                {
                    return NotFound("Business not found");
                }

                if (business.UserId != userId)
                {
                    return Unauthorized("You don't have access");
                }

                var pattern = LocalTimePattern.CreateWithInvariantCulture("HH:mm");
                var localStartTime = pattern.Parse(request.StartTime).Value;
                var localEndTime = pattern.Parse(request.EndTime).Value;
                
                DateTimeZone clientTimeZone;
                try
                {
                    clientTimeZone = DateTimeZoneProviders.Tzdb[request.TimeZone];
                }
                catch (DateTimeZoneNotFoundException)
                {
                    return BadRequest("Invalid time zone");
                }

                var today = LocalDate.FromDateTime(DateTime.Today);
                var localStartDateTime = today.At(localStartTime);
                var localEndDateTime = today.At(localEndTime);

                var zonedStartDateTime = clientTimeZone.AtStrictly(localStartDateTime);
                var zonedEndDateTime = clientTimeZone.AtStrictly(localEndDateTime);

                var utcStartTime = zonedStartDateTime.ToDateTimeUtc().TimeOfDay;
                var utcEndTime = zonedEndDateTime.ToDateTimeUtc().TimeOfDay;

                var dailyWorkingHours = new DailyWorkingHours
                {
                    BusinessId = businessId,
                    Day = request.Day,
                    StartTime = utcStartTime.ToString(@"hh\:mm"),
                    EndTime = utcEndTime.ToString(@"hh\:mm")
                };

                _context.WorkingHours.Add(dailyWorkingHours);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Working hours successfully added" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpGet("{businessId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<DailyWorkingHours>>> GetWorkingHours(int businessId)
        {
            var workingHours = await _context.WorkingHours
            .Where(wh => wh.BusinessId == businessId)
            .ToListAsync();

            if (workingHours == null || workingHours.Count == 0)
            {
                return NotFound("No working hours found for this business.");
            }

            return Ok(workingHours);
        }

        [HttpPut("{businessId}")]
        [Authorize]
        public async Task<IActionResult> UpdateWorkingHours(int businessId, [FromBody] DailyWorkingHoursDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized("You don't have access");
                }
                var userId = int.Parse(userIdClaim);

                var business = await _context.Businesses
                    .Include(b => b.WeeklyWorkingHours)
                    .FirstOrDefaultAsync(b => b.Id == businessId);

                if (business == null)
                {
                    return NotFound("Business not found");
                }

                if (business.Id == businessId)
                {
                    return Forbid("You don't have access");
                }

                var pattern = LocalTimePattern.CreateWithInvariantCulture("HH:mm");
                var localStartTime = pattern.Parse(request.StartTime).Value;
                var localEndTime = pattern.Parse(request.EndTime).Value;

                DateTimeZone clientTimeZone;
                try
                {
                    clientTimeZone = DateTimeZoneProviders.Tzdb[request.TimeZone];
                }
                catch (DateTimeZoneNotFoundException)
                {
                    return BadRequest("Invalid time zone");
                }

                var today = LocalDate.FromDateTime(DateTime.Today);
                var localStartDateTime = today.At(localStartTime);
                var localEndDateTime = today.At(localEndTime);

                var zonedStartDateTime = clientTimeZone.AtStrictly(localStartDateTime);
                var zonedEndDateTime = clientTimeZone.AtStrictly(localEndDateTime);

                var utcStartTime = zonedStartDateTime.ToDateTimeUtc().TimeOfDay;
                var utcEndTime = zonedEndDateTime.ToDateTimeUtc().TimeOfDay;

                var workingHours = await _context.WorkingHours.FirstOrDefaultAsync(wh => wh.BusinessId == business.Id && wh.Day == request.Day);

                if (workingHours == null)
                {
                    return NotFound("WorkingHours not found");
                }

                workingHours.StartTime = utcStartTime.ToString(@"hh\:mm");
                workingHours.EndTime = utcEndTime.ToString(@"hh\:mm");

                _context.WorkingHours.Update(workingHours);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Workign hours successfully updated" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{dailyWorkingHoursId}")]
        [Authorize]
        public async Task<IActionResult> DeleteWorkingHours(int dailyWorkingHoursId)
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

                var workingHours = await _context.WorkingHours.FirstOrDefaultAsync(wh => wh.Id == dailyWorkingHoursId);

                if (workingHours == null)
                {
                    return NotFound("WorkingHours not found");
                }

                if (workingHours.Business.UserId != userId)
                {
                    return Unauthorized("You don't have access");
                }

                _context.WorkingHours.Remove(workingHours);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Working hours deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        public class DailyWorkingHoursDto
        {
            [Required]
            public DayOfWeek Day { get; set; }

            [Required]
            public string StartTime { get; set; }

            [Required]
            public string EndTime { get; set; }
            [Required]
            public string TimeZone { get; set; }
        }
    }
}
