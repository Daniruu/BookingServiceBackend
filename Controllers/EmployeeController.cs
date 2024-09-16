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

        [HttpPost("{businessId}")]
        [Authorize]
        public async Task<IActionResult> AddEmployee(int businessId, [FromBody] EmployeeDto request)
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

                if (business == null)
                {
                    return BadRequest("Business not found.");
                }

                if (business.UserId != userId)
                {
                    return Forbid("You don't have access.");
                }

                if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Role))
                {
                    return BadRequest("Employee name and position cannot be empty.");
                }

                var employee = new Employee
                {
                    BusinessId = business.Id,
                    Name = request.Name,
                    Role = request.Role
                };

                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Employee added successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{employeeId}")]
        [Authorize]
        public async Task<IActionResult> UpdateEmployee(int employeeId, EmployeeDto request)
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

                if (business.UserId != userId)
                {
                    return Forbid("You don't have access.");
                }

                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId && e.BusinessId == business.Id);

                if (employee == null)
                {
                    return NotFound("Employee not found.");
                }

                employee.Name = request.Name;
                employee.Role = request.Role;

                _context.Employees.Update(employee);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Employee updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{employeeId}")]
        [Authorize]
        public async Task<IActionResult> DeleteEmployee(int employeeId)
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

                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId && e.BusinessId == business.Id);

                if (employee == null)
                {
                    return NotFound("Employee not found.");
                }

                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Employee deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{businessId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEmployeeByBusinessId(int businessId)
        {
            try
            {
                var employees = await _context.Employees
                    .Where(e => e.BusinessId == businessId)
                    .ToListAsync();

                if (employees == null || employees.Count == 0)
                {
                    return NotFound("Employees not found.");
                }

                return Ok(employees);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        public class EmployeeDto
        {
            [Required]
            public string Name { get; set; }
            [Required]
            public string Role { get; set; }
        }
    }
}
