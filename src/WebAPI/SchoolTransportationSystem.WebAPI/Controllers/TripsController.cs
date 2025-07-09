using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Rihla.Application.Interfaces;
using Rihla.Application.DTOs;

namespace Rihla.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TripsController : ControllerBase
    {
        private readonly ITripService _tripService;

        public TripsController(ITripService tripService)
        {
            _tripService = tripService;
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
    }
}

