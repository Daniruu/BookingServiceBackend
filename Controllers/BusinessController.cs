using BookingServiceBackend.Data;
using BookingServiceBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime.TimeZones;
using NodaTime;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using NodaTime;
using static BookingServiceBackend.Controllers.EmployeeController;
using static BookingServiceBackend.Controllers.ServiceController;
using NodaTime.Text;
using System;

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
        public async Task<IActionResult> CreateBusiness([FromBody] CreateBusinessDto request)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized("Dostęp zablokowany.");
                }
                var userId = int.Parse(userIdClaim);

                var existingBusiness = await _context.Businesses
                   .FirstOrDefaultAsync(b => b.Name == request.Name || b.Email == request.Email || b.PhoneNumber == request.PhoneNumber);

                if (existingBusiness != null)
                {
                    if (existingBusiness.Name == request.Name)
                        return BadRequest("Biznes z taką nazwą już istnieje.");

                    if (existingBusiness.Email == request.Email)
                        return BadRequest("Biznes z takim adresem Email już istnieje.");

                    if (existingBusiness.PhoneNumber == request.PhoneNumber)
                        return BadRequest("Biznes z takim numerem telefonu już istnieje.");
                }

                var business = new Business
                {
                    UserId = userId,
                    Category = request.Category,
                    Name = request.Name,
                    Email = request.Email,
                    Address = request.Address,
                    PhoneNumber = request.PhoneNumber,
                    Nip = request.Nip,
                    Regon = request.Regon,
                    Krs = request.Krs
                };

                _context.Businesses.Add(business);
                await _context.SaveChangesAsync();

                return Ok(business);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateBusinessData([FromQuery] string timeZone, [FromBody] CreateBusinessDto request)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized("Dostęp zablokowany.");
                }
                var userId = int.Parse(userIdClaim);

                var business = await _context.Businesses
                    .Include(b => b.Address)
                    .Include(b => b.WeeklyWorkingHours)
                    .Include(b => b.Employees)
                    .Include(b => b.Services)
                    .FirstOrDefaultAsync(b => b.UserId == userId);

                if (business == null)
                {
                    return NotFound();
                }

                var existingBusiness = await _context.Businesses
                    .FirstOrDefaultAsync(b =>
                        (b.Name == request.Name || b.Email == request.Email || b.PhoneNumber == request.PhoneNumber)
                        && b.Id != business.Id);

                if (existingBusiness != null)
                {
                    if (existingBusiness.Name == request.Name)
                        return BadRequest("Biznes z taką nazwą już istnieje.");

                    if (existingBusiness.Email == request.Email)
                        return BadRequest("Biznes z takim adresem Email już istnieje.");

                    if (existingBusiness.PhoneNumber == request.PhoneNumber)
                        return BadRequest("Biznes z takim numerem telefonu już istnieje.");
                }

                business.Category = request.Category;
                business.Name = request.Name;
                business.Email = request.Email;
                business.Address = request.Address;
                business.PhoneNumber = request.PhoneNumber;
                business.Nip = request.Nip;
                business.Regon = request.Regon;
                business.Krs = request.Krs;
                

                _context.Businesses.Update(business);
                await _context.SaveChangesAsync();

                DateTimeZone clientTimeZone;
                try
                {
                    clientTimeZone = DateTimeZoneProviders.Tzdb[timeZone];
                }
                catch (DateTimeZoneNotFoundException)
                {
                    return BadRequest("Invalid time zone");
                }

                var businessDto = new BusinessDto
                {
                    Id = business.Id,
                    Name = business.Name,
                    Category = business.Category,
                    Email = business.Email,
                    PhoneNumber = business.PhoneNumber,
                    Nip = business.Nip,
                    Regon = business.Regon,
                    Krs = business.Krs,
                    Address = new AddressDto
                    {
                        Region = business.Address.Region,
                        City = business.Address.City,
                        Street = business.Address.Street,
                        BuildingNumber = business.Address.BuildingNumber,
                        RoomNumber = business.Address.RoomNumber,
                        PostalCode = business.Address.PostalCode
                    },
                    Employees = business.Employees.Select(e => new EmployeeDto
                    {
                        Id = e.Id,
                        Name = e.Name,
                        Role = e.Role
                    }).ToList(),
                    Services = business.Services.Select(s => new ServiceDto
                    {
                        Id = s.Id,
                        EmployeeId = s.EmployeeId,
                        Name = s.Name,
                        Description = s.Description,
                        Price = s.Price,
                        Duration = s.Duration,
                        IsFeatured = s.IsFeatured,
                        Group = s.Group
                    }).ToList(),
                    WeeklyWorkingHours = business.WeeklyWorkingHours.Select(wh => new DailyWorkingHoursDto
                    {
                        Id = wh.Id,
                        Day = wh.Day,
                        StartTime = ConvertUtcToLocalTime(wh.StartTime, clientTimeZone),
                        EndTime = ConvertUtcToLocalTime(wh.EndTime, clientTimeZone)
                    }).ToList()
                };

                return Ok(businessDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("my-business")]
        [Authorize]
        public async Task<IActionResult> GetMyBusiness([FromQuery] string timeZone)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized("You don't have access");
                }
                var userId = int.Parse(userIdClaim);

            
                var business = await _context.Businesses
                    .Include(b => b.Address)
                    .Include(b => b.WeeklyWorkingHours)
                    .Include(b => b.Employees)
                    .Include(b => b.Services)
                    .FirstOrDefaultAsync(b => b.UserId == userId);

                if (business == null)
                {
                    return NotFound();
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

                var businessDto = new BusinessDto
                {
                    Id = business.Id,
                    Name = business.Name,
                    Category = business.Category,
                    Email = business.Email,
                    PhoneNumber = business.PhoneNumber,
                    Nip = business.Nip,
                    Regon = business.Regon,
                    Krs = business.Krs,
                    Address = new AddressDto
                    {
                        Region = business.Address.Region,
                        City = business.Address.City,
                        Street = business.Address.Street,
                        BuildingNumber = business.Address.BuildingNumber,
                        RoomNumber = business.Address.RoomNumber,
                        PostalCode = business.Address.PostalCode
                    },
                    Employees = business.Employees.Select(e => new EmployeeDto
                    {
                        Id = e.Id,
                        Name = e.Name,
                        Role = e.Role
                    }).ToList(),
                    Services = business.Services.Select(s => new ServiceDto
                    {
                        Id = s.Id,
                        EmployeeId = s.EmployeeId,
                        Name = s.Name,
                        Description = s.Description,
                        Price = s.Price,
                        Duration = s.Duration,
                        IsFeatured = s.IsFeatured,
                        Group = s.Group
                    }).ToList(),
                    WeeklyWorkingHours = business.WeeklyWorkingHours.Select(wh => new DailyWorkingHoursDto
                    {
                        Id = wh.Id,
                        Day = wh.Day,
                        StartTime = ConvertUtcToLocalTime(wh.StartTime, clientTimeZone),
                        EndTime = ConvertUtcToLocalTime(wh.EndTime, clientTimeZone)
                    }).ToList()
                };

                return Ok(businessDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        // Получение списка бизнесов с фильтрацией по категории и городу, а так же поддержкой пагинации 
        public async Task<IActionResult> GetBusinesses([FromQuery] string? city = null, [FromQuery] string? category = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {

            try
            {
                var query = _context.Businesses.AsQueryable();

                if (!string.IsNullOrEmpty(city))
                {
                    query = query.Where(b => b.Address.City.ToLower().Contains(city.ToLower()));
                }

                if (!string.IsNullOrEmpty(category))
                {
                    query = query.Where(b => b.Category.ToLower().Contains(category.ToLower()));
                }

                var totalBusinesses = await query.CountAsync();

                if (totalBusinesses == 0)
                {
                    return NotFound("No companies matching this filter were found");
                }

                var businesses = await query
                    .AsNoTracking()
                    .Select( b => new
                    {
                        b.Id,
                        b.Name,
                        b.Address,
                        b.Category,
                        Services = b.Services.Where(s => s.IsFeatured).ToList()
                    })
                    .Skip((page -1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var result = new
                {
                    TotalBusinesses = totalBusinesses,
                    Page = page,
                    PageSize = pageSize,
                    Data = businesses
                };

                return Ok(result);

            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{businessId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBusiness(int businessId, [FromQuery] string timeZone)
        {
            try
            {
                var business = await _context.Businesses
                    .Include(b => b.Address)
                    .Include(b => b.WeeklyWorkingHours)
                    .Include(b => b.Employees)
                    .Include(b => b.Services)
                    .FirstOrDefaultAsync(b => b.Id == businessId);

                if (business == null)
                {
                    return NotFound();
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

                var businessDto = new BusinessDto
                {
                    Id = business.Id,
                    Name = business.Name,
                    Category = business.Category,
                    Email = business.Email,
                    PhoneNumber = business.PhoneNumber,
                    Nip = business.Nip,
                    Regon = business.Regon,
                    Krs = business.Krs,
                    Address = new AddressDto
                    {
                        Region = business.Address.Region,
                        City = business.Address.City,
                        Street = business.Address.Street,
                        BuildingNumber = business.Address.BuildingNumber,
                        RoomNumber = business.Address.RoomNumber,
                        PostalCode = business.Address.PostalCode
                    },
                    Employees = business.Employees.Select(e => new EmployeeDto
                    {
                        Id = e.Id,
                        Name = e.Name,
                        Role = e.Role
                    }).ToList(),
                    Services = business.Services.Select(s => new ServiceDto
                    {
                        Id = s.Id,
                        EmployeeId = s.EmployeeId,
                        Name = s.Name,
                        Description = s.Description,
                        Price = s.Price,
                        Duration = s.Duration,
                        IsFeatured = s.IsFeatured,
                        Group = s.Group
                    }).ToList(),
                    WeeklyWorkingHours = business.WeeklyWorkingHours.Select(wh => new DailyWorkingHoursDto
                    {
                        Id = wh.Id,
                        Day = wh.Day,
                        StartTime = ConvertUtcToLocalTime(wh.StartTime, clientTimeZone),
                        EndTime = ConvertUtcToLocalTime(wh.EndTime, clientTimeZone)
                    }).ToList()
                };

                return Ok(businessDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("exists")]
        public async Task<IActionResult> CheckBusinessExists()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("You don't have access");
            }
            var userId = int.Parse(userIdClaim);

            var business = await _context.Businesses.FirstOrDefaultAsync(b => b.UserId == userId);

            if (business != null)
            {
                return Ok(new { exists = true });
            }
            else
            {
                return Ok(new { exists = false });
            }
        }

        private string ConvertUtcToLocalTime(string utcTime, DateTimeZone clientTimeZone)
        {
            var pattern = LocalTimePattern.CreateWithInvariantCulture("HH:mm");
            var utcLocalTime = pattern.Parse(utcTime).Value;

            var todayUtc = LocalDateTime.FromDateTime(DateTime.UtcNow);
            var utcDateTime = new LocalDateTime(todayUtc.Year, todayUtc.Month, todayUtc.Day, utcLocalTime.Hour, utcLocalTime.Minute);

            var utcZonedDateTime = utcDateTime.InZoneLeniently(DateTimeZone.Utc);

            var clientZonedDateTime = utcZonedDateTime.WithZone(clientTimeZone);

            return clientZonedDateTime.ToString("HH:mm", null);
        }
        public class CreateBusinessDto
        {
            [Required]
            public string Name { get; set; }
            [Required]

            public string Category { get; set; }

            [Required]
            public string Email { get; set; }

            [Required]
            public string PhoneNumber { get; set; }

            [Required]
            public string Nip { get; set; }

            [Required]
            public string Regon { get; set; }

            public string Krs { get; set; }

            [Required]
            public Address Address { get; set; }

        }

        public class BusinessDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Category { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public string Nip { get; set; }
            public string Regon { get; set; }
            public string Krs { get; set; }
            public AddressDto Address { get; set; }
            public List<DailyWorkingHoursDto> WeeklyWorkingHours { get; set; }
            public List<EmployeeDto> Employees { get; set; }
            public List<ServiceDto> Services { get; set; }   
        }

        public class DailyWorkingHoursDto
        {
            public int Id { get; set; }
            public DayOfWeek Day { get; set; }
            public string StartTime { get; set; }
            public string EndTime { get; set; }
        }

        public class EmployeeDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Role { get; set; }
        }

        public class ServiceDto
        {
            public int Id { get; set; }

            public int EmployeeId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public decimal Price { get; set; }
            public int Duration { get; set; }
            public bool IsFeatured { get; set; }
            public string Group { get; set; }
        }

        public class AddressDto
        {
            public string Region { get; set; }
            public string City { get; set; }
            public string Street { get; set; }
            public string BuildingNumber { get; set; }
            public string RoomNumber { get; set; }
            public string PostalCode { get; set; }
        }
    }
}
