using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rihla.Application.DTOs;
using Rihla.Application.Interfaces;
using Rihla.Core.Common;
using Rihla.Core.Entities;
using Rihla.Core.Enums;
using Rihla.Infrastructure.Data;

namespace Rihla.Application.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AttendanceService> _logger;
        private readonly IUserContext _userContext;

        public AttendanceService(ApplicationDbContext context, ILogger<AttendanceService> logger, IUserContext userContext)
        {
            _context = context;
            _logger = logger;
            _userContext = userContext;
        }

        public async Task<Result<AttendanceDto>> GetByIdAsync(int id, string tenantId)
        {
            try
            {
                var attendance = await _context.Attendances
                    .Include(a => a.Student)
                    .Include(a => a.Trip)
                        .ThenInclude(t => t.Route)
                    .Where(a => a.Id == id && a.TenantId == int.Parse(tenantId) && !a.IsDeleted)
                    .FirstOrDefaultAsync();

                if (attendance == null)
                {
                    return Result<AttendanceDto>.Failure("Attendance record not found");
                }

                var attendanceDto = MapToDto(attendance);
                return Result<AttendanceDto>.Success(attendanceDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting attendance by ID {AttendanceId}", id);
                return Result<AttendanceDto>.Failure("An error occurred while retrieving the attendance record");
            }
        }

        public async Task<Result<PagedResult<AttendanceDto>>> GetAllAsync(AttendanceSearchDto searchDto, string tenantId)
        {
            try
            {
                var query = _context.Attendances
                    .Include(a => a.Student)
                    .Include(a => a.Trip)
                        .ThenInclude(t => t.Route)
                    .Where(a => a.TenantId == int.Parse(tenantId) && !a.IsDeleted);

                if (searchDto.StudentId.HasValue)
                    query = query.Where(a => a.StudentId == searchDto.StudentId.Value);

                if (searchDto.TripId.HasValue)
                    query = query.Where(a => a.TripId == searchDto.TripId.Value);

                if (searchDto.RouteId.HasValue)
                    query = query.Where(a => a.Trip.RouteId == searchDto.RouteId.Value);

                if (searchDto.Status.HasValue)
                    query = query.Where(a => a.Status == searchDto.Status.Value);

                if (searchDto.AttendanceDateFrom.HasValue)
                    query = query.Where(a => a.Date.Date >= searchDto.AttendanceDateFrom.Value.Date);

                if (searchDto.AttendanceDateTo.HasValue)
                    query = query.Where(a => a.Date.Date <= searchDto.AttendanceDateTo.Value.Date);

                var totalCount = await query.CountAsync();

                var attendances = await query
                    .OrderByDescending(a => a.Date)
                    .Skip((searchDto.Page - 1) * searchDto.PageSize)
                    .Take(searchDto.PageSize)
                    .ToListAsync();

                var attendanceDtos = attendances.Select(MapToDto).ToList();

                var pagedResult = new PagedResult<AttendanceDto>
                {
                    Items = attendanceDtos,
                    TotalCount = totalCount,
                    Page = searchDto.Page,
                    PageSize = searchDto.PageSize
                };

                return Result<PagedResult<AttendanceDto>>.Success(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting attendance records");
                return Result<PagedResult<AttendanceDto>>.Failure("An error occurred while retrieving attendance records");
            }
        }

        public async Task<Result<AttendanceDto>> CreateAsync(CreateAttendanceDto createDto, string tenantId)
        {
            try
            {
                var student = await _context.Students
                    .Where(s => s.Id == createDto.StudentId && s.TenantId == int.Parse(tenantId) && !s.IsDeleted)
                    .FirstOrDefaultAsync();

                if (student == null)
                {
                    return Result<AttendanceDto>.Failure("Student not found");
                }

                var trip = await _context.Trips
                    .Where(t => t.Id == createDto.TripId && t.TenantId == int.Parse(tenantId) && !t.IsDeleted)
                    .FirstOrDefaultAsync();

                if (trip == null)
                {
                    return Result<AttendanceDto>.Failure("Trip not found");
                }

                var existingAttendance = await _context.Attendances
                    .Where(a => a.StudentId == createDto.StudentId && a.TripId == createDto.TripId && !a.IsDeleted)
                    .FirstOrDefaultAsync();

                if (existingAttendance != null)
                {
                    return Result<AttendanceDto>.Failure("Attendance record already exists for this student and trip");
                }

                var attendance = new Attendance
                {
                    TenantId = int.Parse(tenantId),
                    StudentId = createDto.StudentId,
                    TripId = createDto.TripId,
                    Date = createDto.AttendanceDate,
                    Status = createDto.Status,
                    BoardingTime = createDto.BoardingTime,
                    AlightingTime = createDto.AlightingTime,
                    Notes = createDto.Notes,
                    RecordedBy = _userContext.GetUsername() ?? "System",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Attendances.Add(attendance);
                await _context.SaveChangesAsync();

                var attendanceDto = MapToDto(attendance);
                return Result<AttendanceDto>.Success(attendanceDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating attendance record");
                return Result<AttendanceDto>.Failure("An error occurred while creating the attendance record");
            }
        }

        public async Task<Result<AttendanceDto>> UpdateAsync(int id, UpdateAttendanceDto updateDto, string tenantId)
        {
            try
            {
                var attendance = await _context.Attendances
                    .Where(a => a.Id == id && a.TenantId == int.Parse(tenantId) && !a.IsDeleted)
                    .FirstOrDefaultAsync();

                if (attendance == null)
                {
                    return Result<AttendanceDto>.Failure("Attendance record not found");
                }

                attendance.Status = updateDto.Status;
                attendance.BoardingTime = updateDto.BoardingTime;
                attendance.AlightingTime = updateDto.AlightingTime;
                attendance.Notes = updateDto.Notes;
                attendance.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var attendanceDto = MapToDto(attendance);
                return Result<AttendanceDto>.Success(attendanceDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating attendance record with ID {AttendanceId}", id);
                return Result<AttendanceDto>.Failure("An error occurred while updating the attendance record");
            }
        }

        public async Task<Result<bool>> DeleteAsync(int id, string tenantId)
        {
            try
            {
                var attendance = await _context.Attendances
                    .Where(a => a.Id == id && a.TenantId == int.Parse(tenantId) && !a.IsDeleted)
                    .FirstOrDefaultAsync();

                if (attendance == null)
                {
                    return Result<bool>.Failure("Attendance record not found");
                }

                attendance.IsDeleted = true;
                attendance.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting attendance record with ID {AttendanceId}", id);
                return Result<bool>.Failure("An error occurred while deleting the attendance record");
            }
        }

        public async Task<Result<List<AttendanceDto>>> GetAttendanceByStudentAsync(int studentId, DateTime startDate, DateTime endDate, string tenantId)
        {
            try
            {
                var attendances = await _context.Attendances
                    .Include(a => a.Student)
                    .Include(a => a.Trip)
                        .ThenInclude(t => t.Route)
                    .Where(a => a.StudentId == studentId && a.TenantId == int.Parse(tenantId) && !a.IsDeleted)
                    .Where(a => a.Date.Date >= startDate.Date && a.Date.Date <= endDate.Date)
                    .OrderBy(a => a.Date)
                    .ToListAsync();

                var attendanceDtos = attendances.Select(MapToDto).ToList();
                return Result<List<AttendanceDto>>.Success(attendanceDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting attendance by student {StudentId} from {StartDate} to {EndDate}", studentId, startDate, endDate);
                return Result<List<AttendanceDto>>.Failure("An error occurred while retrieving student attendance records");
            }
        }

        public async Task<Result<List<AttendanceDto>>> GetAttendanceByTripAsync(int tripId, string tenantId)
        {
            try
            {
                var attendances = await _context.Attendances
                    .Include(a => a.Student)
                    .Include(a => a.Trip)
                        .ThenInclude(t => t.Route)
                    .Where(a => a.TripId == tripId && a.TenantId == int.Parse(tenantId) && !a.IsDeleted)
                    .OrderBy(a => a.Student.FullName.LastName)
                    .ThenBy(a => a.Student.FullName.FirstName)
                    .ToListAsync();

                var attendanceDtos = attendances.Select(MapToDto).ToList();
                return Result<List<AttendanceDto>>.Success(attendanceDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting attendance by trip {TripId}", tripId);
                return Result<List<AttendanceDto>>.Failure("An error occurred while retrieving trip attendance records");
            }
        }

        public async Task<Result<bool>> RecordBoardingAsync(int studentId, int tripId, int stopId, DateTime boardingTime, string tenantId)
        {
            try
            {
                var attendance = await _context.Attendances
                    .Where(a => a.StudentId == studentId && a.TripId == tripId && a.TenantId == int.Parse(tenantId) && !a.IsDeleted)
                    .FirstOrDefaultAsync();

                if (attendance == null)
                {
                    attendance = new Attendance
                    {
                        TenantId = int.Parse(tenantId),
                        StudentId = studentId,
                        TripId = tripId,
                        Date = boardingTime.Date,
                        Status = AttendanceStatus.Present,
                        BoardingTime = boardingTime,
                        RecordedBy = _userContext.GetUsername() ?? "System",
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Attendances.Add(attendance);
                }
                else
                {
                    attendance.BoardingTime = boardingTime;
                    attendance.Status = AttendanceStatus.Present;
                    attendance.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording boarding for student {StudentId} on trip {TripId}", studentId, tripId);
                return Result<bool>.Failure("An error occurred while recording boarding");
            }
        }

        public async Task<Result<bool>> RecordAlightingAsync(int studentId, int tripId, int stopId, DateTime alightingTime, string tenantId)
        {
            try
            {
                var attendance = await _context.Attendances
                    .Where(a => a.StudentId == studentId && a.TripId == tripId && a.TenantId == int.Parse(tenantId) && !a.IsDeleted)
                    .FirstOrDefaultAsync();

                if (attendance == null)
                {
                    return Result<bool>.Failure("No boarding record found for this student and trip");
                }

                attendance.AlightingTime = alightingTime;
                attendance.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording alighting for student {StudentId} on trip {TripId}", studentId, tripId);
                return Result<bool>.Failure("An error occurred while recording alighting");
            }
        }

        private AttendanceDto MapToDto(Attendance attendance)
        {
            return new AttendanceDto
            {
                Id = attendance.Id,
                StudentId = attendance.StudentId,
                TripId = attendance.TripId,
                RouteStopId = 0, // Not available in entity
                AttendanceDate = attendance.Date,
                Status = attendance.Status,
                BoardingTime = attendance.BoardingTime,
                AlightingTime = attendance.AlightingTime,
                BoardingLatitude = null, // Not available in entity
                BoardingLongitude = null, // Not available in entity
                AlightingLatitude = null, // Not available in entity
                AlightingLongitude = null, // Not available in entity
                Notes = attendance.Notes,
                ParentNotified = false, // Not available in entity
                ParentNotificationTime = null, // Not available in entity
                ExceptionReason = null, // Not available in entity
                CreatedAt = attendance.CreatedAt,
                UpdatedAt = attendance.UpdatedAt,
                TenantId = attendance.TenantId.ToString(),
                Student = attendance.Student != null ? MapStudentToDto(attendance.Student) : new StudentDto(),
                Trip = attendance.Trip != null ? MapTripToDto(attendance.Trip) : new TripDto(),
                RouteStop = new RouteStopDto() // Not available in entity
            };
        }

        private StudentDto MapStudentToDto(Student student)
        {
            return new StudentDto
            {
                Id = student.Id,
                TenantId = student.TenantId,
                StudentNumber = student.StudentNumber,
                FirstName = student.FullName.FirstName,
                LastName = student.FullName.LastName,
                MiddleName = student.FullName.MiddleName,
                DateOfBirth = student.DateOfBirth,
                Grade = student.Grade,
                Phone = student.Phone,
                Email = student.Email,
                Street = student.Address.Street,
                City = student.Address.City,
                State = student.Address.State,
                ZipCode = student.Address.ZipCode,
                Country = student.Address.Country,
                ParentName = student.ParentName,
                ParentPhone = student.ParentPhone,
                ParentEmail = student.ParentEmail,
                EmergencyContact = student.EmergencyContact,
                EmergencyPhone = student.EmergencyPhone,
                SpecialNeeds = student.SpecialNeeds,
                Notes = student.Notes,
                CreatedAt = student.CreatedAt,
                UpdatedAt = student.UpdatedAt
            };
        }

        private TripDto MapTripToDto(Trip trip)
        {
            return new TripDto
            {
                Id = trip.Id,
                RouteId = trip.RouteId,
                VehicleId = trip.VehicleId,
                DriverId = trip.DriverId,
                TripDate = trip.ScheduledStartTime.Date,
                Type = TripType.PickUp, // Default since not in entity
                Status = trip.Status,
                ScheduledStartTime = trip.ScheduledStartTime,
                ActualStartTime = trip.ActualStartTime,
                ScheduledEndTime = trip.ScheduledEndTime,
                ActualEndTime = trip.ActualEndTime,
                ActualDistance = trip.GetTripDistance(),
                FuelConsumed = null, // Not available in entity
                StudentsPickedUp = 0, // Would need to calculate from attendance
                StudentsDroppedOff = 0, // Would need to calculate from attendance
                Notes = trip.Notes,
                IncidentReports = null, // Not available in entity
                CreatedAt = trip.CreatedAt,
                UpdatedAt = trip.UpdatedAt,
                TenantId = trip.TenantId.ToString(),
                Route = trip.Route != null ? MapRouteToDto(trip.Route) : new RouteDto()
            };
        }

        private RouteDto MapRouteToDto(Route route)
        {
            return new RouteDto
            {
                Id = route.Id,
                TenantId = route.TenantId.ToString(),
                RouteNumber = route.Id.ToString(),
                Name = route.Name,
                Description = route.Description,
                Type = RouteType.Regular,
                Status = RouteStatus.Active,
                StartTime = TimeSpan.FromHours(7),
                EndTime = TimeSpan.FromHours(8),
                EstimatedDuration = TimeSpan.FromMinutes(60),
                EstimatedDistance = 10.0m,
                MaxCapacity = 50,
                CurrentOccupancy = 0,
                IsActive = true, // Default since not in entity
                CreatedAt = route.CreatedAt,
                UpdatedAt = route.UpdatedAt
            };
        }
    }
}

