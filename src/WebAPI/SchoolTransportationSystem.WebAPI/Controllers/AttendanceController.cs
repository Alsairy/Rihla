using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SchoolTransportationSystem.Application.Interfaces;
using SchoolTransportationSystem.Application.DTOs;

namespace SchoolTransportationSystem.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AttendanceDto>>> GetAttendance([FromQuery] AttendanceSearchDto searchDto)
        {
            try
            {
                var tenantId = "1";
                var result = await _attendanceService.GetAllAsync(searchDto ?? new AttendanceSearchDto(), tenantId);
                
                if (!result.IsSuccess)
                    return StatusCode(500, new { message = "Error retrieving attendance records", error = result.Error });

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving attendance records", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AttendanceDto>> GetAttendanceRecord(int id)
        {
            try
            {
                var tenantId = "1";
                var result = await _attendanceService.GetByIdAsync(id, tenantId);
                
                if (!result.IsSuccess)
                    return NotFound(new { message = $"Attendance record with ID {id} not found" });

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving attendance record", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<AttendanceDto>> CreateAttendanceRecord([FromBody] CreateAttendanceDto createAttendanceDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var tenantId = "1";
                createAttendanceDto.TenantId = tenantId;
                var result = await _attendanceService.CreateAsync(createAttendanceDto, tenantId);
                
                if (!result.IsSuccess)
                    return StatusCode(500, new { message = "Error creating attendance record", error = result.Error });

                return CreatedAtAction(nameof(GetAttendanceRecord), new { id = result.Value.Id }, result.Value);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating attendance record", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAttendanceRecord(int id, [FromBody] UpdateAttendanceDto updateAttendanceDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var tenantId = "1";
                updateAttendanceDto.Id = id;
                var result = await _attendanceService.UpdateAsync(id, updateAttendanceDto, tenantId);
                
                if (!result.IsSuccess)
                    return NotFound(new { message = $"Attendance record with ID {id} not found" });

                return Ok(new { message = "Attendance record updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating attendance record", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAttendanceRecord(int id)
        {
            try
            {
                var tenantId = "1";
                var result = await _attendanceService.DeleteAsync(id, tenantId);
                
                if (!result.IsSuccess)
                    return NotFound(new { message = $"Attendance record with ID {id} not found" });

                return Ok(new { message = "Attendance record deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting attendance record", error = ex.Message });
            }
        }

        [HttpGet("statistics")]
        public async Task<ActionResult> GetAttendanceStatistics()
        {
            try
            {
                var stats = new
                {
                    TotalStudents = 0,
                    PresentToday = 0,
                    AbsentToday = 0,
                    LateToday = 0,
                    AttendanceRate = 0.0
                };
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving attendance statistics", error = ex.Message });
            }
        }

        [HttpPost("rfid")]
        public async Task<ActionResult<AttendanceDto>> RecordRFIDAttendance([FromBody] RFIDAttendanceDto rfidAttendanceDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var tenantId = "1";
                var result = await _attendanceService.RecordRFIDAttendanceAsync(rfidAttendanceDto.RFIDTag, rfidAttendanceDto.TripId, rfidAttendanceDto.RouteStopId, rfidAttendanceDto.Timestamp, tenantId);
                
                if (!result.IsSuccess)
                    return StatusCode(500, new { message = "Error recording RFID attendance", error = result.Error });

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error recording RFID attendance", error = ex.Message });
            }
        }

        [HttpPost("photo")]
        public async Task<ActionResult<AttendanceDto>> RecordPhotoAttendance([FromBody] PhotoAttendanceDto photoAttendanceDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var tenantId = "1";
                var result = await _attendanceService.RecordPhotoAttendanceAsync(photoAttendanceDto.StudentId, photoAttendanceDto.TripId, photoAttendanceDto.RouteStopId, photoAttendanceDto.PhotoBase64, photoAttendanceDto.Timestamp, tenantId);
                
                if (!result.IsSuccess)
                    return StatusCode(500, new { message = "Error recording photo attendance", error = result.Error });

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error recording photo attendance", error = ex.Message });
            }
        }

        [HttpPost("biometric")]
        public async Task<ActionResult<AttendanceDto>> RecordBiometricAttendance([FromBody] BiometricAttendanceDto biometricAttendanceDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var tenantId = "1";
                var result = await _attendanceService.RecordBiometricAttendanceAsync(biometricAttendanceDto.StudentId, biometricAttendanceDto.TripId, biometricAttendanceDto.RouteStopId, biometricAttendanceDto.BiometricData, biometricAttendanceDto.BiometricType, biometricAttendanceDto.Timestamp, tenantId);
                
                if (!result.IsSuccess)
                    return StatusCode(500, new { message = "Error recording biometric attendance", error = result.Error });

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error recording biometric attendance", error = ex.Message });
            }
        }

        [HttpPost("sync-offline")]
        public async Task<ActionResult<OfflineAttendanceSyncResultDto>> SyncOfflineAttendance([FromBody] OfflineAttendanceSyncDto syncDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var tenantId = "1";
                var result = await _attendanceService.SyncOfflineAttendanceAsync(syncDto.AttendanceRecords, tenantId);
                
                if (!result.IsSuccess)
                    return StatusCode(500, new { message = "Error syncing offline attendance", error = result.Error });

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error syncing offline attendance", error = ex.Message });
            }
        }

        [HttpGet("methods")]
        public async Task<ActionResult<IEnumerable<AttendanceMethodDto>>> GetAttendanceMethods()
        {
            try
            {
                var tenantId = "1";
                var result = await _attendanceService.GetAttendanceMethodsAsync(tenantId);
                
                if (!result.IsSuccess)
                    return StatusCode(500, new { message = "Error retrieving attendance methods", error = result.Error });

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving attendance methods", error = ex.Message });
            }
        }

        [HttpGet("analytics")]
        public async Task<ActionResult<AttendanceAnalyticsDto>> GetAttendanceAnalytics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var tenantId = "1";
                var result = await _attendanceService.GetAttendanceAnalyticsAsync(startDate, endDate, tenantId);
                
                if (!result.IsSuccess)
                    return StatusCode(500, new { message = "Error retrieving attendance analytics", error = result.Error });

                return Ok(result.Value);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving attendance analytics", error = ex.Message });
            }
        }
    }
}

