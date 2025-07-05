using Microsoft.AspNetCore.Mvc;
using SchoolTransportationSystem.Application.DTOs;
using SchoolTransportationSystem.Application.Services;
using Microsoft.AspNetCore.Authorization;

namespace SchoolTransportationSystem.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RoutesController : ControllerBase
    {
        private readonly RouteService _routeService;

        public RoutesController(RouteService routeService)
        {
            _routeService = routeService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RouteDto>>> GetRoutes()
        {
            try
            {
                var routes = await _routeService.GetAllRoutesAsync();
                return Ok(routes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving routes", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RouteDto>> GetRoute(int id)
        {
            try
            {
                var route = await _routeService.GetRouteByIdAsync(id);
                if (route == null)
                {
                    return NotFound(new { message = $"Route with ID {id} not found" });
                }
                return Ok(route);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving route", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<RouteDto>> CreateRoute(CreateRouteDto createRouteDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var route = await _routeService.CreateRouteAsync(createRouteDto);
                return CreatedAtAction(nameof(GetRoute), new { id = route.Id }, route);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating route", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<RouteDto>> UpdateRoute(int id, UpdateRouteDto updateRouteDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var route = await _routeService.UpdateRouteAsync(id, updateRouteDto);
                if (route == null)
                {
                    return NotFound(new { message = $"Route with ID {id} not found" });
                }
                return Ok(route);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating route", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoute(int id)
        {
            try
            {
                var success = await _routeService.DeleteRouteAsync(id);
                if (!success)
                {
                    return NotFound(new { message = $"Route with ID {id} not found" });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting route", error = ex.Message });
            }
        }

        [HttpGet("statistics")]
        public async Task<ActionResult<object>> GetRouteStatistics()
        {
            try
            {
                var stats = await _routeService.GetRouteStatisticsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving route statistics", error = ex.Message });
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<RouteDto>>> SearchRoutes([FromQuery] string searchTerm)
        {
            try
            {
                var routes = await _routeService.SearchRoutesAsync(searchTerm);
                return Ok(routes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error searching routes", error = ex.Message });
            }
        }

        [HttpPost("{id}/assign-vehicle")]
        public async Task<ActionResult> AssignVehicle(int id, [FromBody] int vehicleId)
        {
            try
            {
                var success = await _routeService.AssignVehicleToRouteAsync(id, vehicleId);
                if (!success)
                {
                    return NotFound(new { message = $"Route with ID {id} not found" });
                }
                return Ok(new { message = "Vehicle assigned successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error assigning vehicle", error = ex.Message });
            }
        }

        [HttpPost("{id}/assign-driver")]
        public async Task<ActionResult> AssignDriver(int id, [FromBody] int driverId)
        {
            try
            {
                var success = await _routeService.AssignDriverToRouteAsync(id, driverId);
                if (!success)
                {
                    return NotFound(new { message = $"Route with ID {id} not found" });
                }
                return Ok(new { message = "Driver assigned successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error assigning driver", error = ex.Message });
            }
        }

        [HttpGet("{id}/students")]
        public async Task<ActionResult<IEnumerable<object>>> GetRouteStudents(int id)
        {
            try
            {
                var students = await _routeService.GetRouteStudentsAsync(id);
                return Ok(students);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving route students", error = ex.Message });
            }
        }

        [HttpPost("{id}/optimize")]
        public async Task<ActionResult<object>> OptimizeRoute(int id)
        {
            try
            {
                var optimizedRoute = await _routeService.OptimizeRouteAsync(id);
                return Ok(optimizedRoute);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error optimizing route", error = ex.Message });
            }
        }
    }
}

