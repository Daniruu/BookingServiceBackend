using BookingServiceBackend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookingServiceBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly BookignServiceDbContext _context;

        public UserController(BookignServiceDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetUser()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized();
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(userId));
                if (user == null)
                {
                    return NotFound();
                }

                return Ok(new { user.Email, user.FirstName, user.SecondName, user.PhoneNumber });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser(UserDto request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return Unauthorized();
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(userId));
                if (user == null)
                {
                    return NotFound();
                }

                user.Email = request.Email;
                user.FirstName = request.FirstName;
                user.SecondName = request.SecondName;
                user.PhoneNumber = request.PhoneNumber;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Dane pomyślnie zaktualizowane" });

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        public class UserDto()
        {
            
            public string FirstName { get; set; }
            public string SecondName { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
        }
    }
}
