using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolTransportationSystem.Infrastructure.Data;
using SchoolTransportationSystem.Core.Entities;
using SchoolTransportationSystem.Core.Enums;

namespace SchoolTransportationSystem.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehiclesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<VehiclesController> _logger;

        public VehiclesController(ApplicationDbContext context, ILogger<VehiclesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetVehicles()
        {
            try
            {
                var vehicles = await _context.Vehicles
                    .Where(v => !v.IsDeleted)
                    .Select(v => new
                    {
                        v.Id,
                        v.VehicleNumber,
                        v.LicensePlate,
                        v.Make,
                        v.Model,
                        v.Year,
                        v.Capacity,
                        v.Status,
                        v.Mileage,
                        v.CreatedAt
                    })
                    .ToListAsync();

                return Ok(vehicles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching vehicles");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetVehicle(int id)
        {
            try
            {
                var vehicle = await _context.Vehicles
                    .Where(v => v.Id == id && !v.IsDeleted)
                    .Select(v => new
                    {
                        v.Id,
                        v.VehicleNumber,
                        v.LicensePlate,
                        v.Make,
                        v.Model,
                        v.Year,
                        v.Capacity,
                        v.Status,
                        v.Mileage,
                        v.CreatedAt,
                        v.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                if (vehicle == null)
                {
                    return NotFound();
                }

                return Ok(vehicle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching vehicle {VehicleId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<ActionResult<object>> CreateVehicle([FromBody] CreateVehicleDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var vehicle = new Vehicle
                {
                    TenantId = 1, // Use default tenant ID for now
                    VehicleNumber = createDto.VehicleNumber,
                    LicensePlate = createDto.LicensePlate,
                    Make = createDto.Make,
                    Model = createDto.Model,
                    Year = createDto.Year,
                    Capacity = createDto.Capacity,
                    Status = createDto.Status,
                    Mileage = createDto.Mileage,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Vehicles.Add(vehicle);
                await _context.SaveChangesAsync();

                var result = new
                {
                    vehicle.Id,
                    vehicle.VehicleNumber,
                    vehicle.LicensePlate,
                    vehicle.Make,
                    vehicle.Model,
                    vehicle.Year,
                    vehicle.Capacity,
                    vehicle.Status,
                    vehicle.Mileage,
                    vehicle.CreatedAt
                };

                return CreatedAtAction(nameof(GetVehicle), new { id = vehicle.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating vehicle");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVehicle(int id, [FromBody] UpdateVehicleDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var vehicle = await _context.Vehicles
                    .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);

                if (vehicle == null)
                {
                    return NotFound();
                }

                vehicle.LicensePlate = updateDto.LicensePlate;
                vehicle.Make = updateDto.Make;
                vehicle.Model = updateDto.Model;
                vehicle.Year = updateDto.Year;
                vehicle.Capacity = updateDto.Capacity;
                vehicle.Status = updateDto.Status;
                vehicle.Mileage = updateDto.Mileage;
                vehicle.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle {VehicleId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            try
            {
                var vehicle = await _context.Vehicles
                    .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);

                if (vehicle == null)
                {
                    return NotFound();
                }

                vehicle.IsDeleted = true;
                vehicle.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vehicle {VehicleId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("statistics")]
        public async Task<ActionResult<object>> GetVehicleStatistics()
        {
            try
            {
                var totalVehicles = await _context.Vehicles.CountAsync(v => !v.IsDeleted);
                var activeVehicles = await _context.Vehicles.CountAsync(v => !v.IsDeleted && v.Status == VehicleStatus.Active);
                var maintenanceVehicles = await _context.Vehicles.CountAsync(v => !v.IsDeleted && v.Status == VehicleStatus.Maintenance);
                var outOfServiceVehicles = await _context.Vehicles.CountAsync(v => !v.IsDeleted && v.Status == VehicleStatus.OutOfService);

                var statistics = new
                {
                    TotalVehicles = totalVehicles,
                    ActiveVehicles = activeVehicles,
                    MaintenanceVehicles = maintenanceVehicles,
                    OutOfServiceVehicles = outOfServiceVehicles,
                    UtilizationRate = totalVehicles > 0 ? (double)activeVehicles / totalVehicles * 100 : 0
                };

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching vehicle statistics");
                return StatusCode(500, "Internal server error");
            }
        }
    }

    public class CreateVehicleDto
    {
        public string VehicleNumber { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Capacity { get; set; }
        public VehicleStatus Status { get; set; } = VehicleStatus.Active;
        public int Mileage { get; set; }
    }

    public class UpdateVehicleDto
    {
        public string LicensePlate { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Capacity { get; set; }
        public VehicleStatus Status { get; set; }
        public int Mileage { get; set; }
    }
}

