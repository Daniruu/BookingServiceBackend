using BookingServiceBackend.Data;
using BookingServiceBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace BookingServiceBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ServiceController : ControllerBase
    {
        private readonly BookignServiceDbContext _context;

        public ServiceController(BookignServiceDbContext context)
        {
            _context = context;
        }

        [HttpPost("{businessId}")]
        [Authorize]
        public async Task<IActionResult> CreateService(int businessId, [FromBody] ServiceDto request)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized("You don't have access");
                }
                var userId = int.Parse(userIdClaim);

                var business = await _context.Businesses.FirstOrDefaultAsync(b => b.Id == businessId);

                if (business == null || business.UserId != userId)
                {
                    return Forbid("You don't have access to add a service for this business.");
                }

                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == request.EmployeeId);

                if (employee == null)
                {
                    return BadRequest("Employee not found");
                }

                var service = new Service
                {
                    BusinessId = business.Id,
                    EmployeeId = request.EmployeeId,
                    Name = request.Name,
                    Description = request.Description,
                    Price = request.Price,
                    Duration = request.Duration,
                    Group = string.IsNullOrEmpty(request.Group) ? "Pozostałe usługi" : request.Group
                };

                _context.Services.Add(service);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Service successfully added" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{serviceId}")]
        [Authorize]
        public async Task<IActionResult> UpdateService(int serviceId, [FromBody] ServiceDto request)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("You don't have access.");
            }
            var userId = int.Parse(userIdClaim);

            var business = await _context.Businesses.FirstOrDefaultAsync(b => b.UserId == userId);

            if (business == null)
            {
                return BadRequest("Business not found.");
            }

            var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == serviceId);

            if (service == null)
            {
                return NotFound("Service not found.");
            }

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == service.EmployeeId && e.BusinessId == business.Id);
            if (employee == null)
            {
                return Forbid("You don't have access to delete this service.");
            }

            service.EmployeeId = request.EmployeeId;
            service.Name = request.Name;
            service.Description = request.Description;
            service.Price = request.Price;
            service.Duration = request.Duration;
            service.Group = request.Group;

            _context.Services.Update(service);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Service updated successfully" });
        }

        [HttpPut("toggle-featured-status/{serviceId}")]
        [Authorize]
        public async Task<IActionResult> ToggleServiceFeaturedStatus(int serviceId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("You don't have access.");
            }
            var userId = int.Parse(userIdClaim);

            var business = await _context.Businesses.FirstOrDefaultAsync(b => b.UserId == userId);

            if (business == null)
            {
                return BadRequest("Business not found.");
            }

            var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == serviceId);

            if (service == null)
            {
                return NotFound("Service not found.");
            }

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == service.EmployeeId && e.BusinessId == business.Id);
            if (employee == null)
            {
                return Forbid("You don't have access to delete this service.");
            }

            service.IsFeatured = !service.IsFeatured;

            _context.Services.Update(service);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Service updated successfully" });
        }

        [HttpDelete("{serviceId}")]
        [Authorize]
        public async Task<IActionResult> DeleteService(int serviceId)
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

                var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == serviceId);

                if (service == null)
                {
                    return NotFound("Service not found.");
                }

                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == service.EmployeeId && e.BusinessId == business.Id);

                if (employee == null)
                {
                    return Forbid("You don't have access to delete this service.");
                }

                _context.Services.Remove(service);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Service deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{businessId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetServicesByBusinessId([FromRoute] int businessId)
        {
            var employees = await _context.Employees
                .Where(e => e.BusinessId == businessId)
                .ToListAsync();

            if (employees == null || employees.Count == 0)
            {
                return NotFound("Employees not found.");
            }

            var services = await _context.Services
                .Where(s => employees.Select(e => e.Id).Contains(s.EmployeeId))
                .ToListAsync();

            if (services == null || services.Count == 0)
            {
                return NotFound("Services not found.");
            }

            var serviceDtos = services.Select(s => new
            {
                s.Id,
                s.Name,
                s.Description,
                s.Price,
                s.Duration,
                EmployeeName = employees.FirstOrDefault(e => e.Id == s.EmployeeId)?.Name
            }).ToList();

            return Ok(serviceDtos);
        }

        public class ServiceDto
        {
            [Required]
            public int EmployeeId { get; set; }
            [Required]
            public string Name { get; set; }
            [Required]
            public string Description { get; set; }
            [Required]
            public decimal Price { get; set; }
            [Required]
            public int Duration { get; set; }

            public string Group {  get; set; }
        }
    }
}
