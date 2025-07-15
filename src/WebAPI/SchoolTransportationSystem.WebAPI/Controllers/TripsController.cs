using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SchoolTransportationSystem.Application.Interfaces;
using SchoolTransportationSystem.Application.DTOs;
using SchoolTransportationSystem.Infrastructure.Data;
using SchoolTransportationSystem.Core.Enums;

namespace SchoolTransportationSystem.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TripsController : ControllerBase
    {
        private readonly ITripService _tripService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TripsController> _logger;

        public TripsController(ITripService tripService, ApplicationDbContext context, ILogger<TripsController> logger)
        {
            _tripService = tripService;
            _context = context;
            _logger = logger;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TripDto>>> GetTrips([FromQuery] TripSearchDto searchDto)
        {
            try
            {
                var tenantId = "1";
                var result = await _tripService.GetAllAsync(searchDto ?? new TripSearchDto(), tenantId);
                
                if (!result.IsSuccess)
                    return StatusCode(500, new { message = "Error retrieving trips", error = result.Error });

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving trips", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TripDto>> GetTrip(int id)
        {
            try
            {
                var tenantId = "1";
                var result = await _tripService.GetByIdAsync(id, tenantId);
                
                if (!result.IsSuccess)
                    return NotFound(new { message = $"Trip with ID {id} not found" });

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving trip", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<TripDto>> CreateTrip([FromBody] CreateTripDto createTripDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var tenantId = "1";
                createTripDto.TenantId = tenantId;
                var result = await _tripService.CreateAsync(createTripDto, tenantId);
                
                if (!result.IsSuccess)
                    return StatusCode(500, new { message = "Error creating trip", error = result.Error });

                return CreatedAtAction(nameof(GetTrip), new { id = result.Value.Id }, result.Value);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating trip", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTrip(int id, [FromBody] UpdateTripDto updateTripDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var tenantId = "1";
                updateTripDto.Id = id;
                var result = await _tripService.UpdateAsync(id, updateTripDto, tenantId);
                
                if (!result.IsSuccess)
                    return NotFound(new { message = $"Trip with ID {id} not found" });

                return Ok(new { message = "Trip updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating trip", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTrip(int id)
        {
            try
            {
                var tenantId = "1";
                var result = await _tripService.DeleteAsync(id, tenantId);
                
                if (!result.IsSuccess)
                    return NotFound(new { message = $"Trip with ID {id} not found" });

                return Ok(new { message = "Trip deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting trip", error = ex.Message });
            }
        }

        [HttpGet("statistics")]
        public async Task<ActionResult> GetTripStatistics()
        {
            try
            {
                var stats = new
                {
                    TotalTrips = 0,
                    CompletedTrips = 0,
                    OngoingTrips = 0,
                    CancelledTrips = 0,
                    AveragePassengers = 0.0,
                    OnTimePercentage = 0.0
                };
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving trip statistics", error = ex.Message });
            }
        }

        [HttpGet("my-trips")]
        public async Task<ActionResult<IEnumerable<TripDto>>> GetMyTrips()
        {
            try
            {
                var tenantId = "1";
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                if (!int.TryParse(userId, out int driverIdInt))
                    return BadRequest("Invalid user ID format");

                var trips = await _context.Trips
                    .Where(t => !t.IsDeleted && t.TenantId == int.Parse(tenantId) && t.DriverId == driverIdInt)
                    .Include(t => t.Route)
                    .Include(t => t.Vehicle)
                    .Select(t => new TripDto
                    {
                        Id = t.Id,
                        RouteId = t.RouteId,
                        VehicleId = t.VehicleId,
                        DriverId = t.DriverId,
                        Status = t.Status,
                        ScheduledStartTime = t.ScheduledStartTime,
                        ScheduledEndTime = t.ScheduledEndTime,
                        ActualStartTime = t.ActualStartTime,
                        ActualEndTime = t.ActualEndTime,
                        Notes = t.Notes,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt,
                        TenantId = t.TenantId.ToString()
                    })
                    .ToListAsync();

                return Ok(trips);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving driver trips for user {UserId}", User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(500, new { message = "Error retrieving trips", error = ex.Message });
            }
        }

        [HttpGet("my-children")]
        public async Task<ActionResult<IEnumerable<TripDto>>> GetMyChildrenTrips()
        {
            try
            {
                var tenantId = "1";
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                if (!int.TryParse(userId, out int parentIdInt))
                    return BadRequest("Invalid user ID format");

                var trips = await _context.Trips
                    .Where(t => !t.IsDeleted && t.TenantId == int.Parse(tenantId) && 
                               t.Route.Students.Any(s => s.ParentId == parentIdInt))
                    .Include(t => t.Route)
                    .Include(t => t.Vehicle)
                    .Select(t => new TripDto
                    {
                        Id = t.Id,
                        RouteId = t.RouteId,
                        VehicleId = t.VehicleId,
                        DriverId = t.DriverId,
                        Status = t.Status,
                        ScheduledStartTime = t.ScheduledStartTime,
                        ScheduledEndTime = t.ScheduledEndTime,
                        ActualStartTime = t.ActualStartTime,
                        ActualEndTime = t.ActualEndTime,
                        Notes = t.Notes,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt,
                        TenantId = t.TenantId.ToString()
                    })
                    .ToListAsync();

                return Ok(trips);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving children trips for parent {UserId}", User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(500, new { message = "Error retrieving children trips", error = ex.Message });
            }
        }

        [HttpPost("{id}/start")]
        public async Task<IActionResult> StartTrip(int id)
        {
            try
            {
                var tenantId = "1";
                var trip = await _context.Trips
                    .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted && t.TenantId == int.Parse(tenantId));

                if (trip == null)
                    return NotFound();

                trip.Status = TripStatus.InProgress;
                trip.ActualStartTime = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Trip started successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting trip {TripId}", id);
                return StatusCode(500, new { message = "Error starting trip", error = ex.Message });
            }
        }

        [HttpPost("{id}/complete")]
        public async Task<IActionResult> CompleteTrip(int id)
        {
            try
            {
                var tenantId = "1";
                var trip = await _context.Trips
                    .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted && t.TenantId == int.Parse(tenantId));

                if (trip == null)
                    return NotFound();

                trip.Status = TripStatus.Completed;
                trip.ActualEndTime = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Trip completed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing trip {TripId}", id);
                return StatusCode(500, new { message = "Error completing trip", error = ex.Message });
            }
        }

        [HttpPost("generate-daily-schedule")]
        public async Task<ActionResult<IEnumerable<TripDto>>> GenerateDailyTripSchedule([FromBody] DailyScheduleRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var tenantId = "1";
                var result = await _tripService.GenerateDailyTripScheduleAsync(request, tenantId);
                
                if (!result.IsSuccess)
                    return StatusCode(500, new { message = "Error generating daily trip schedule", error = result.Error });

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating daily trip schedule");
                return StatusCode(500, new { message = "Error generating daily trip schedule", error = ex.Message });
            }
        }

        [HttpPut("{id}/reschedule")]
        public async Task<ActionResult<TripDto>> RescheduleTrip(int id, [FromBody] RescheduleTripDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var tenantId = "1";
                var result = await _tripService.RescheduleTripAsync(id, request, tenantId);
                
                if (!result.IsSuccess)
                    return StatusCode(500, new { message = "Error rescheduling trip", error = result.Error });

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rescheduling trip {TripId}", id);
                return StatusCode(500, new { message = "Error rescheduling trip", error = ex.Message });
            }
        }

        [HttpGet("schedule-conflicts")]
        public async Task<ActionResult<IEnumerable<ScheduleConflictDto>>> GetScheduleConflicts([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var tenantId = "1";
                var result = await _tripService.GetScheduleConflictsAsync(startDate, endDate, tenantId);
                
                if (!result.IsSuccess)
                    return StatusCode(500, new { message = "Error retrieving schedule conflicts", error = result.Error });

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving schedule conflicts");
                return StatusCode(500, new { message = "Error retrieving schedule conflicts", error = ex.Message });
            }
        }

        [HttpPost("optimize-schedule")]
        public async Task<ActionResult<ScheduleOptimizationResultDto>> OptimizeSchedule([FromBody] ScheduleOptimizationRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var tenantId = "1";
                var result = await _tripService.OptimizeScheduleAsync(request, tenantId);
                
                if (!result.IsSuccess)
                    return StatusCode(500, new { message = "Error optimizing schedule", error = result.Error });

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing schedule");
                return StatusCode(500, new { message = "Error optimizing schedule", error = ex.Message });
            }
        }

        [HttpGet("schedule-analytics")]
        public async Task<ActionResult<ScheduleAnalyticsDto>> GetScheduleAnalytics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var tenantId = "1";
                var result = await _tripService.GetScheduleAnalyticsAsync(startDate, endDate, tenantId);
                
                if (!result.IsSuccess)
                    return StatusCode(500, new { message = "Error retrieving schedule analytics", error = result.Error });

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving schedule analytics");
                return StatusCode(500, new { message = "Error retrieving schedule analytics", error = ex.Message });
            }
        }
    }
}

