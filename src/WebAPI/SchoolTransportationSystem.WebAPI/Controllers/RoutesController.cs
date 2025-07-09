using Microsoft.AspNetCore.Mvc;
using Rihla.Application.DTOs;
using Rihla.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Rihla.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ManagerOrAbove")]
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

    }
}

