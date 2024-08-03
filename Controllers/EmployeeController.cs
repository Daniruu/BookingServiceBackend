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
    public class EmployeeController : ControllerBase
    {
        private readonly BookignServiceDbContext _context;

        public EmployeeController(BookignServiceDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddEmployee(EmployeeDto request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var business = await _context.Businesses.FirstOrDefaultAsync(b => b.UserId == userId);

            if (business == null) 
            {
                return BadRequest("Business not found for the current user.");
            }

            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Position))
            {
                return BadRequest("Employee name and position cannot be empty");
            }

            var employee = new Employee
            {
                BusinessId = business.Id,
                Name = request.Name,
                Position = request.Position
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Employee added successfully." });
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateEmployee(int id, EmployeeDto request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var business = await _context.Businesses.FirstOrDefaultAsync(b => b.UserId == userId);

            if (business == null)
            {
                return BadRequest("Business not found for the current user.");
            }

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == id && e.BusinessId == business.Id);

            if (employee == null)
            {
                return NotFound("Employee not found.");
            }

            employee.Name = request.Name;
            employee.Position = request.Position;

            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Employee updated successfully" });
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var business = await _context.Businesses.FirstOrDefaultAsync(b => b.UserId == userId);

            if (business == null)
            {
                return BadRequest("Business not found for the current user.");
            }

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == id && e.BusinessId == business.Id);

            if (employee == null)
            {
                return NotFound("Employee not found.");
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Employee deleted successfully" });
        }

        [HttpGet("by-bussiness/{businessId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEmployeeByBusiness(int businessId)
        {
            var employees = await _context.Employees
                .Where(e => e.BusinessId == businessId)
                .ToListAsync();

            if (employees == null || employees.Count == 0)
            {
                return NotFound();
            }

            return Ok(employees);
        }

        public class EmployeeDto
        {
            [Required]
            public string Name { get; set; }
            [Required]
            public string Position { get; set; }
        }
    }
}
