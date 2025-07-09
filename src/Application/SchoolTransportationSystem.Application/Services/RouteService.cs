using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolTransportationSystem.Application.DTOs;
using SchoolTransportationSystem.Application.Interfaces;
using SchoolTransportationSystem.Core.Common;
using SchoolTransportationSystem.Core.Entities;
using SchoolTransportationSystem.Core.Enums;
using SchoolTransportationSystem.Infrastructure.Data;

namespace SchoolTransportationSystem.Application.Services
{
    public class RouteService : IRouteService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RouteService> _logger;

        public RouteService(ApplicationDbContext context, ILogger<RouteService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<RouteDto>> GetByIdAsync(int id, string tenantId)
        {
            try
            {
                var route = await _context.Routes
                    .Include(r => r.AssignedVehicle)
                    .Include(r => r.AssignedDriver)
                    .Include(r => r.RouteStops)
                    .Include(r => r.Students)
                    .Where(r => r.Id == id && r.TenantId == int.Parse(tenantId) && !r.IsDeleted)
                    .FirstOrDefaultAsync();

                if (route == null)
                {
                    return Result<RouteDto>.Failure("Route not found");
                }

                var routeDto = MapToDto(route);
                return Result<RouteDto>.Success(routeDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting route by ID {RouteId}", id);
                return Result<RouteDto>.Failure("An error occurred while retrieving the route");
            }
        }

        public async Task<Result<PagedResult<RouteDto>>> GetAllAsync(RouteSearchDto searchDto, string tenantId)
        {
            try
            {
                var query = _context.Routes
                    .Include(r => r.AssignedVehicle)
                    .Include(r => r.AssignedDriver)
                    .Where(r => r.TenantId == int.Parse(tenantId) && !r.IsDeleted);

                if (!string.IsNullOrEmpty(searchDto.RouteNumber))
                {
                    query = query.Where(r => r.RouteNumber.Contains(searchDto.RouteNumber));
                }

                if (!string.IsNullOrEmpty(searchDto.Name))
                {
                    query = query.Where(r => r.Name.Contains(searchDto.Name));
                }


                if (searchDto.Status.HasValue)
                {
                    query = query.Where(r => r.Status == searchDto.Status.Value);
                }

                if (searchDto.VehicleId.HasValue)
                {
                    query = query.Where(r => r.AssignedVehicleId == searchDto.VehicleId.Value);
                }

                if (searchDto.DriverId.HasValue)
                {
                    query = query.Where(r => r.AssignedDriverId == searchDto.DriverId.Value);
                }


                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / searchDto.PageSize);

                var routes = await query
                    .Skip((searchDto.Page - 1) * searchDto.PageSize)
                    .Take(searchDto.PageSize)
                    .ToListAsync();

                var routeDtos = routes.Select(MapToDto).ToList();

                var pagedResult = new PagedResult<RouteDto>
                {
                    Items = routeDtos,
                    TotalCount = totalCount,
                    Page = searchDto.Page,
                    PageSize = searchDto.PageSize,
                    TotalPages = totalPages
                };

                return Result<PagedResult<RouteDto>>.Success(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting routes");
                return Result<PagedResult<RouteDto>>.Failure("An error occurred while retrieving routes");
            }
        }

        public async Task<Result<RouteDto>> CreateAsync(CreateRouteDto createDto, string tenantId)
        {
            try
            {
                var existingRoute = await _context.Routes
                    .Where(r => r.RouteNumber == createDto.RouteNumber && r.TenantId == int.Parse(tenantId) && !r.IsDeleted)
                    .FirstOrDefaultAsync();

                if (existingRoute != null)
                {
                    return Result<RouteDto>.Failure("Route number already exists");
                }

                var route = new Route
                {
                    RouteNumber = createDto.RouteNumber,
                    Name = createDto.Name,
                    Description = createDto.Description ?? string.Empty,
                    Status = createDto.Status,
                    StartTime = createDto.StartTime,
                    EndTime = createDto.EndTime,
                    Distance = createDto.EstimatedDistance,
                    EstimatedDuration = (int)createDto.EstimatedDuration.TotalMinutes,
                    StartLocation = string.Empty,
                    EndLocation = string.Empty,
                    Notes = createDto.Notes,
                    TenantId = int.Parse(tenantId),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Routes.Add(route);
                await _context.SaveChangesAsync();

                var routeDto = MapToDto(route);
                return Result<RouteDto>.Success(routeDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating route");
                return Result<RouteDto>.Failure("An error occurred while creating the route");
            }
        }

        public async Task<Result<RouteDto>> UpdateAsync(int id, UpdateRouteDto updateDto, string tenantId)
        {
            try
            {
                var route = await _context.Routes
                    .Where(r => r.Id == id && r.TenantId == int.Parse(tenantId) && !r.IsDeleted)
                    .FirstOrDefaultAsync();

                if (route == null)
                {
                    return Result<RouteDto>.Failure("Route not found");
                }

                var existingRoute = await _context.Routes
                    .Where(r => r.RouteNumber == updateDto.RouteNumber && r.Id != id && r.TenantId == int.Parse(tenantId) && !r.IsDeleted)
                    .FirstOrDefaultAsync();

                if (existingRoute != null)
                {
                    return Result<RouteDto>.Failure("Route number already exists");
                }

                route.RouteNumber = updateDto.RouteNumber;
                route.Name = updateDto.Name;
                route.Description = updateDto.Description ?? string.Empty;
                route.Status = updateDto.Status;
                route.StartTime = updateDto.StartTime;
                route.EndTime = updateDto.EndTime;
                route.Distance = updateDto.EstimatedDistance;
                route.EstimatedDuration = (int)updateDto.EstimatedDuration.TotalMinutes;
                route.Notes = updateDto.Notes;
                route.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var routeDto = MapToDto(route);
                return Result<RouteDto>.Success(routeDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating route {RouteId}", id);
                return Result<RouteDto>.Failure("An error occurred while updating the route");
            }
        }

        public async Task<Result<bool>> DeleteAsync(int id, string tenantId)
        {
            try
            {
                var route = await _context.Routes
                    .Where(r => r.Id == id && r.TenantId == int.Parse(tenantId) && !r.IsDeleted)
                    .FirstOrDefaultAsync();

                if (route == null)
                {
                    return Result<bool>.Failure("Route not found");
                }

                route.IsDeleted = true;
                route.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting route {RouteId}", id);
                return Result<bool>.Failure("An error occurred while deleting the route");
            }
        }

        public async Task<Result<RouteDto>> GetByRouteNumberAsync(string routeNumber, string tenantId)
        {
            try
            {
                var route = await _context.Routes
                    .Include(r => r.AssignedVehicle)
                    .Include(r => r.AssignedDriver)
                    .Where(r => r.RouteNumber == routeNumber && r.TenantId == int.Parse(tenantId) && !r.IsDeleted)
                    .FirstOrDefaultAsync();

                if (route == null)
                {
                    return Result<RouteDto>.Failure("Route not found");
                }

                var routeDto = MapToDto(route);
                return Result<RouteDto>.Success(routeDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting route by number {RouteNumber}", routeNumber);
                return Result<RouteDto>.Failure("An error occurred while retrieving the route");
            }
        }

        public async Task<Result<List<RouteDto>>> GetActiveRoutesAsync(string tenantId)
        {
            try
            {
                var routes = await _context.Routes
                    .Include(r => r.AssignedVehicle)
                    .Include(r => r.AssignedDriver)
                    .Where(r => r.TenantId == int.Parse(tenantId) && !r.IsDeleted && r.Status == RouteStatus.Active)
                    .ToListAsync();

                var routeDtos = routes.Select(MapToDto).ToList();
                return Result<List<RouteDto>>.Success(routeDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active routes");
                return Result<List<RouteDto>>.Failure("An error occurred while retrieving active routes");
            }
        }

        public async Task<Result<List<StudentDto>>> GetStudentsOnRouteAsync(int routeId, string tenantId)
        {
            try
            {
                var route = await _context.Routes
                    .Where(r => r.Id == routeId && r.TenantId == int.Parse(tenantId) && !r.IsDeleted)
                    .FirstOrDefaultAsync();

                if (route == null)
                {
                    return Result<List<StudentDto>>.Failure("Route not found");
                }

                var students = await _context.Students
                    .Where(s => s.RouteId == routeId && !s.IsDeleted)
                    .ToListAsync();

                var studentDtos = students.Select(s => new StudentDto
                {
                    Id = s.Id,
                    StudentNumber = s.StudentNumber,
                    FirstName = s.FullName.FirstName,
                    LastName = s.FullName.LastName,
                    MiddleName = s.FullName.MiddleName,
                    Grade = s.Grade,
                    Status = s.Status,
                    RouteId = s.RouteId
                }).ToList();

                return Result<List<StudentDto>>.Success(studentDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting students on route {RouteId}", routeId);
                return Result<List<StudentDto>>.Failure("An error occurred while retrieving students on route");
            }
        }

        private RouteDto MapToDto(Route route)
        {
            return new RouteDto
            {
                Id = route.Id,
                RouteNumber = route.RouteNumber,
                Name = route.Name,
                Description = route.Description,
                Status = route.Status,
                StartTime = route.StartTime,
                EndTime = route.EndTime,
                EstimatedDistance = route.Distance,
                EstimatedDuration = TimeSpan.FromMinutes(route.EstimatedDuration),
                Notes = route.Notes,
                IsActive = true,
                CreatedAt = route.CreatedAt,
                UpdatedAt = route.UpdatedAt,
                TenantId = route.TenantId.ToString(),
                AssignedVehicle = route.AssignedVehicle != null ? new VehicleDto
                {
                    Id = route.AssignedVehicle.Id,
                    VehicleNumber = route.AssignedVehicle.VehicleNumber,
                    LicensePlate = route.AssignedVehicle.LicensePlate,
                    Make = route.AssignedVehicle.Make,
                    Model = route.AssignedVehicle.Model,
                    Capacity = route.AssignedVehicle.Capacity
                } : null,
                AssignedDriver = route.AssignedDriver != null ? new DriverDto
                {
                    Id = route.AssignedDriver.Id,
                    EmployeeNumber = route.AssignedDriver.EmployeeNumber,
                    FirstName = route.AssignedDriver.FullName.FirstName,
                    LastName = route.AssignedDriver.FullName.LastName,
                    MiddleName = route.AssignedDriver.FullName.MiddleName,
                    Phone = route.AssignedDriver.Phone
                } : null
            };
        }
    }
}

