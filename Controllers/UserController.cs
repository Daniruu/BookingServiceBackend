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
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                if (email == null)
                {
                    return Unauthorized();
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    return NotFound();
                }

                return Ok(new { user.Email, user.UserName, user.PhoneNumber });
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
                user.UserName = request.UserName;
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
            public string Email { get; set; }
            public string UserName { get; set; }
            public string PhoneNumber { get; set; }
        }
    }
}
