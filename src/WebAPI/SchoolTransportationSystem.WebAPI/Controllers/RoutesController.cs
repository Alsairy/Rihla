using Microsoft.AspNetCore.Mvc;
using SchoolTransportationSystem.Application.DTOs;
using SchoolTransportationSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace SchoolTransportationSystem.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RoutesController : ControllerBase
    {
        private readonly IRouteService _routeService;
        private readonly IUserContext _userContext;

        public RoutesController(IRouteService routeService, IUserContext userContext)
        {
            _routeService = routeService;
            _userContext = userContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RouteDto>>> GetRoutes([FromQuery] RouteSearchDto searchDto)
        {
            var tenantId = _userContext.GetTenantId().ToString();
            var result = await _routeService.GetAllAsync(searchDto ?? new RouteSearchDto(), tenantId);
            
            if (!result.IsSuccess)
                return StatusCode(500, new { message = "Error retrieving routes", error = result.Error });
                
            return Ok(result.Value.Items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RouteDto>> GetRoute(int id)
        {
            var tenantId = _userContext.GetTenantId().ToString();
            var result = await _routeService.GetByIdAsync(id, tenantId);
            
            if (!result.IsSuccess)
                return NotFound(new { message = $"Route with ID {id} not found" });
                
            return Ok(result.Value);
        }

        [HttpPost]
        public async Task<ActionResult<RouteDto>> CreateRoute(CreateRouteDto createRouteDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tenantId = _userContext.GetTenantId().ToString();
            var result = await _routeService.CreateAsync(createRouteDto, tenantId);
            
            if (!result.IsSuccess)
                return StatusCode(500, new { message = "Error creating route", error = result.Error });
                
            return CreatedAtAction(nameof(GetRoute), new { id = result.Value.Id }, result.Value);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<RouteDto>> UpdateRoute(int id, UpdateRouteDto updateRouteDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tenantId = _userContext.GetTenantId().ToString();
            var result = await _routeService.UpdateAsync(id, updateRouteDto, tenantId);
            
            if (!result.IsSuccess)
                return NotFound(new { message = $"Route with ID {id} not found" });
                
            return Ok(result.Value);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoute(int id)
        {
            var tenantId = _userContext.GetTenantId().ToString();
            var result = await _routeService.DeleteAsync(id, tenantId);
            
            if (!result.IsSuccess)
                return NotFound(new { message = $"Route with ID {id} not found" });
                
            return NoContent();
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<RouteDto>>> GetActiveRoutes()
        {
            var tenantId = _userContext.GetTenantId().ToString();
            var result = await _routeService.GetActiveRoutesAsync(tenantId);
            
            if (!result.IsSuccess)
                return StatusCode(500, new { message = "Error retrieving active routes", error = result.Error });
                
            return Ok(result.Value);
        }

        [HttpGet("by-number/{routeNumber}")]
        public async Task<ActionResult<RouteDto>> GetRouteByNumber(string routeNumber)
        {
            var tenantId = _userContext.GetTenantId().ToString();
            var result = await _routeService.GetByRouteNumberAsync(routeNumber, tenantId);
            
            if (!result.IsSuccess)
                return NotFound(new { message = $"Route with number {routeNumber} not found" });
                
            return Ok(result.Value);
        }

        [HttpGet("{id}/students")]
        public async Task<ActionResult<IEnumerable<StudentDto>>> GetRouteStudents(int id)
        {
            var tenantId = _userContext.GetTenantId().ToString();
            var result = await _routeService.GetStudentsOnRouteAsync(id, tenantId);
            
            if (!result.IsSuccess)
                return StatusCode(500, new { message = "Error retrieving route students", error = result.Error });
                
            return Ok(result.Value);
        }

        [HttpPost("{id}/optimize")]
        public async Task<ActionResult<RouteOptimizationDto>> OptimizeRoute(int id, [FromBody] RouteOptimizationRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tenantId = _userContext.GetTenantId().ToString();
            var result = await _routeService.GenerateOptimalRouteAsync(id, request, tenantId);
            
            if (!result.IsSuccess)
                return StatusCode(500, new { message = "Error optimizing route", error = result.Error });
                
            return Ok(result.Value);
        }

        [HttpPut("{id}/optimize")]
        public async Task<ActionResult<RouteOptimizationDto>> OptimizeExistingRoute(int id, [FromBody] RouteOptimizationRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tenantId = _userContext.GetTenantId().ToString();
            var result = await _routeService.OptimizeExistingRouteAsync(id, request, tenantId);
            
            if (!result.IsSuccess)
                return StatusCode(500, new { message = "Error optimizing existing route", error = result.Error });
                
            return Ok(result.Value);
        }

        [HttpGet("{id}/efficiency-metrics")]
        public async Task<ActionResult<RouteEfficiencyMetricsDto>> GetRouteEfficiencyMetrics(int id)
        {
            var tenantId = _userContext.GetTenantId().ToString();
            var result = await _routeService.CalculateRouteEfficiencyMetricsAsync(id, tenantId);
            
            if (!result.IsSuccess)
                return StatusCode(500, new { message = "Error calculating route efficiency metrics", error = result.Error });
                
            return Ok(result.Value);
        }

        [HttpGet("{id}/optimization-history")]
        public async Task<ActionResult<IEnumerable<RouteOptimizationDto>>> GetOptimizationHistory(int id)
        {
            var tenantId = _userContext.GetTenantId().ToString();
            var result = await _routeService.GetOptimizationHistoryAsync(id, tenantId);
            
            if (!result.IsSuccess)
                return StatusCode(500, new { message = "Error retrieving optimization history", error = result.Error });
                
            return Ok(result.Value);
        }

    }
}

