using BookingServiceBackend.Data;
using BookingServiceBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

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

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateService([FromBody] ServiceDto request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == request.EmployeeId);

            if (employee == null)
            {
                return BadRequest("Employee not found");
            }

            var business = await _context.Businesses.FirstOrDefaultAsync(b => b.Id == employee.BusinessId);

            if (business == null || business.UserId != userId)
            {
                return BadRequest("You are not allowed to add services for this business");
            }

            var service = new Service
            {
                EmployeeId = request.EmployeeId,
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Duration = request.Duration
            };

            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Service successfully added" });
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateService(int id, [FromBody] ServiceDto request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == id); 

            if (service == null)
            {
                return NotFound("Service not found");
            }

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == service.EmployeeId);
            var business = await _context.Businesses.FirstOrDefaultAsync(b => b.Id == employee.BusinessId);

            if (business == null || business.UserId != userId)
            {
                return BadRequest("You are not allowed to update services for this business");
            }

            service.Name = request.Name;
            service.Description = request.Description;
            service.Price = request.Price;
            service.Duration = request.Duration;

            _context.Services.Update(service);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Service updated successfully" });
        }

        [HttpGet("{employeeId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetServices([FromRoute] int employeeId)
        {
            var services = await _context.Services
                .Where(s => s.EmployeeId == employeeId)
                .ToListAsync();

            if (services == null || services.Count == 0)
            {
                return NotFound("Services not found");
            }

            return Ok(services);
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
        }
    }
}
