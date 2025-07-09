using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Rihla.Application.Interfaces;
using Rihla.Application.DTOs;

namespace Rihla.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "AdminOnly")]
    public class MaintenanceController : ControllerBase
    {
        private readonly IMaintenanceService _maintenanceService;

        public MaintenanceController(IMaintenanceService maintenanceService)
        {
            _maintenanceService = maintenanceService;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MaintenanceRecordDto>>> GetMaintenanceRecords([FromQuery] MaintenanceSearchDto searchDto)
        {
            try
            {
                var tenantId = "1";
                var result = await _maintenanceService.GetAllAsync(searchDto ?? new MaintenanceSearchDto(), tenantId);
                
                if (!result.IsSuccess)
                    return StatusCode(500, new { message = "Error retrieving maintenance records", error = result.Error });

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving maintenance records", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MaintenanceRecordDto>> GetMaintenanceRecord(int id)
        {
            try
            {
                var tenantId = "1";
                var result = await _maintenanceService.GetByIdAsync(id, tenantId);
                
                if (!result.IsSuccess)
                    return NotFound(new { message = $"Maintenance record with ID {id} not found" });

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving maintenance record", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<MaintenanceRecordDto>> CreateMaintenanceRecord([FromBody] CreateMaintenanceRecordDto createMaintenanceDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var tenantId = "1";
                createMaintenanceDto.TenantId = tenantId;
                var result = await _maintenanceService.CreateAsync(createMaintenanceDto, tenantId);
                
                if (!result.IsSuccess)
                    return StatusCode(500, new { message = "Error creating maintenance record", error = result.Error });

                return CreatedAtAction(nameof(GetMaintenanceRecord), new { id = result.Value.Id }, result.Value);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating maintenance record", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMaintenanceRecord(int id, [FromBody] UpdateMaintenanceRecordDto updateMaintenanceDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var tenantId = "1";
                updateMaintenanceDto.Id = id;
                var result = await _maintenanceService.UpdateAsync(id, updateMaintenanceDto, tenantId);
                
                if (!result.IsSuccess)
                    return NotFound(new { message = $"Maintenance record with ID {id} not found" });

                return Ok(new { message = "Maintenance record updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating maintenance record", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMaintenanceRecord(int id)
        {
            try
            {
                var tenantId = "1";
                var result = await _maintenanceService.DeleteAsync(id, tenantId);
                
                if (!result.IsSuccess)
                    return NotFound(new { message = $"Maintenance record with ID {id} not found" });

                return Ok(new { message = "Maintenance record deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting maintenance record", error = ex.Message });
            }
        }

        [HttpGet("statistics")]
        public async Task<ActionResult> GetMaintenanceStatistics()
        {
            try
            {
                var stats = new
                {
                    ScheduledMaintenance = 0,
                    InProgressMaintenance = 0,
                    OverdueMaintenance = 0,
                    TotalCost = 0.0,
                    AverageCost = 0.0
                };
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving maintenance statistics", error = ex.Message });
            }
        }
    }
}

