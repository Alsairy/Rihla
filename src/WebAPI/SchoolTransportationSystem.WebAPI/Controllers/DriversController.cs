using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SchoolTransportationSystem.Infrastructure.Data;
using SchoolTransportationSystem.Core.Entities;
using SchoolTransportationSystem.Core.ValueObjects;
using SchoolTransportationSystem.Core.Enums;

namespace SchoolTransportationSystem.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DriversController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DriversController> _logger;

        public DriversController(ApplicationDbContext context, ILogger<DriversController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetDrivers()
        {
            try
            {
                var drivers = await _context.Drivers
                    .Where(d => !d.IsDeleted)
                    .Select(d => new
                    {
                        d.Id,
                        DriverNumber = d.EmployeeNumber,
                        Name = d.FullName.FirstName + " " + d.FullName.LastName,
                        d.Phone,
                        d.Email,
                        d.LicenseNumber,
                        LicenseExpiry = d.LicenseExpiry,
                        d.Status,
                        d.CreatedAt
                    })
                    .ToListAsync();

                return Ok(drivers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching drivers");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetDriver(int id)
        {
            try
            {
                var driver = await _context.Drivers
                    .Where(d => d.Id == id && !d.IsDeleted)
                    .Select(d => new
                    {
                        d.Id,
                        DriverNumber = d.EmployeeNumber,
                        FirstName = d.FullName.FirstName,
                        LastName = d.FullName.LastName,
                        d.Phone,
                        d.Email,
                        d.LicenseNumber,
                        LicenseExpiry = d.LicenseExpiry,
                        d.Status,
                        d.CreatedAt,
                        d.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                if (driver == null)
                {
                    return NotFound();
                }

                return Ok(driver);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching driver {DriverId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<ActionResult<object>> CreateDriver([FromBody] CreateDriverDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var driver = new Driver
                {
                    TenantId = 1, // Use default tenant ID for now
                    EmployeeNumber = createDto.DriverNumber,
                    FullName = new FullName(createDto.FirstName, createDto.LastName),
                    Phone = createDto.Phone,
                    Email = createDto.Email,
                    LicenseNumber = createDto.LicenseNumber,
                    LicenseExpiry = createDto.LicenseExpiry,
                    Status = createDto.Status,
                    Address = new Address("", "", "", "", ""), // Default empty address
                    HireDate = DateTime.UtcNow,
                    DateOfBirth = DateTime.UtcNow.AddYears(-30), // Default age
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Drivers.Add(driver);
                await _context.SaveChangesAsync();

                var result = new
                {
                    driver.Id,
                    DriverNumber = driver.EmployeeNumber,
                    Name = driver.FullName.FirstName + " " + driver.FullName.LastName,
                    driver.Phone,
                    driver.Email,
                    driver.LicenseNumber,
                    LicenseExpiry = driver.LicenseExpiry,
                    driver.Status,
                    driver.CreatedAt
                };

                return CreatedAtAction(nameof(GetDriver), new { id = driver.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating driver");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDriver(int id, [FromBody] UpdateDriverDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var driver = await _context.Drivers
                    .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

                if (driver == null)
                {
                    return NotFound();
                }

                driver.FullName = new FullName(updateDto.FirstName, updateDto.LastName);
                driver.Phone = updateDto.Phone;
                driver.Email = updateDto.Email;
                driver.LicenseNumber = updateDto.LicenseNumber;
                driver.LicenseExpiry = updateDto.LicenseExpiry;
                driver.Status = updateDto.Status;
                driver.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating driver {DriverId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDriver(int id)
        {
            try
            {
                var driver = await _context.Drivers
                    .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

                if (driver == null)
                {
                    return NotFound();
                }

                driver.IsDeleted = true;
                driver.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting driver {DriverId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }

    public class CreateDriverDto
    {
        public string DriverNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public DateTime LicenseExpiry { get; set; }
        public DriverStatus Status { get; set; } = DriverStatus.Active;
    }

    public class UpdateDriverDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public DateTime LicenseExpiry { get; set; }
        public DriverStatus Status { get; set; }
    }
}

