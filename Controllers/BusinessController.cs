using BookingServiceBackend.Data;
using BookingServiceBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace BookingServiceBackend.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class BusinessController : ControllerBase
    {
        private readonly BookignServiceDbContext _context;

        public BusinessController(BookignServiceDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateBusiness(BusinessDto request)
        {
            if (await _context.Businesses.AnyAsync(b => b.Name == request.Name))
            {
                return BadRequest("Biznes z taką nazwą już istnieje.");
            }

            if (await _context.Businesses.AnyAsync(b => b.Email == request.Email))
            {
                return BadRequest("Biznes z takim adresem Email już istnieje.");
            }

            if (await _context.Businesses.AnyAsync(b => b.PhoneNumber == request.PhoneNumber))
            {
                return BadRequest("Biznes z takim numerem telefonu Email już istnieje.");
            }

            var business = new Business
            {
                UserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                Category = request.Category,
                Name = request.Name,
                Email = request.Email,
                Address = request.Address,
                PhoneNumber = request.PhoneNumber,
                WorkingHours = request.WorkingHours
            };

            _context.Businesses.Add(business);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Biznes pomyślnie dodany." });
        }

        [HttpGet("business")]
        [Authorize]
        public async Task<IActionResult> GetBusiness()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var business = await _context.Businesses
                    .Include(b => b.Address)
                    .Include(b => b.WorkingHours)
                    .FirstOrDefaultAsync(b => b.UserId == userId);

                if (business == null)
                {
                    return NotFound();
                }

                return Ok(business);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateBusiness(BusinessDto request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var business = await _context.Businesses
                .Include(b => b.Address)
                .Include(b => b.WorkingHours)
                .FirstOrDefaultAsync(b => b.UserId == userId);

            if (business == null)
            {
                return NotFound();
            }

            business.Category = request.Category;
            business.Name = request.Name;
            business.Email = request.Email;
            business.Address = request.Address;
            business.PhoneNumber = request.PhoneNumber;
            business.WorkingHours = request.WorkingHours;

            _context.Businesses.Update(business);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Biznes pomyślnie zaktualizowany." });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllBusinesses([FromQuery] string city, [FromQuery] string category)
        {
            var query = _context.Businesses.AsQueryable();

            try
            {
                if (!string.IsNullOrWhiteSpace(city))
                {
                    query = query.Where(b => b.Address.City == city);
                }

                if (!string.IsNullOrWhiteSpace(category))
                {
                    query = query.Where(b => b.Category == category);
                }

                var businesses = await query
                    .Include(b => b.Address)
                    .Include(b => b.WorkingHours)
                    .ToListAsync();

                return Ok(businesses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBusinessById(int id)
        {
            try
            {
                var business = await _context.Businesses
                .Include(b => b.Address)
                .Include(b => b.WorkingHours)
                .FirstOrDefaultAsync(b => b.Id == id);

                if (business == null)
                {
                    return NotFound();
                }

                return Ok(business);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        public class BusinessDto
        {
            public string Category { get; set; }

            [Required]
            public string Name { get; set; }

            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            public Address Address { get; set; }

            [Required]
            public string PhoneNumber { get; set; }

            [Required]
            public ICollection<WorkingHours> WorkingHours { get; set; }
        }
    }
}
