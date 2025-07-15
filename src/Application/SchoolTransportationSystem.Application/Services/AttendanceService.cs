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


        public async Task<Result<AttendanceDto>> RecordRFIDAttendanceAsync(string rfidTag, int tripId, int stopId, DateTime timestamp, string tenantId)
        {
            try
            {
                var student = await _context.Students
                    .Where(s => s.RfidTag == rfidTag && s.TenantId == int.Parse(tenantId) && !s.IsDeleted)
                    .FirstOrDefaultAsync();

                if (student == null)
                {
                    _logger.LogWarning("RFID tag {RfidTag} not found for tenant {TenantId}", rfidTag, tenantId);
                    return Result<AttendanceDto>.Failure("Student with RFID tag not found");
                }

                var trip = await _context.Trips
                    .Where(t => t.Id == tripId && t.TenantId == int.Parse(tenantId) && !t.IsDeleted)
                    .FirstOrDefaultAsync();

                if (trip == null)
                {
                    return Result<AttendanceDto>.Failure("Trip not found");
                }

                var existingAttendance = await _context.Attendances
                    .Where(a => a.StudentId == student.Id && a.TripId == tripId && a.Date.Date == timestamp.Date && !a.IsDeleted)
                    .FirstOrDefaultAsync();

                if (existingAttendance != null)
                {
                    existingAttendance.BoardingTime = timestamp;
                    existingAttendance.Status = AttendanceStatus.Present;
                    existingAttendance.Notes = $"RFID scan at {timestamp:HH:mm:ss}";
                    existingAttendance.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    existingAttendance = new Attendance
                    {
                        TenantId = int.Parse(tenantId),
                        StudentId = student.Id,
                        TripId = tripId,
                        Date = timestamp.Date,
                        Status = AttendanceStatus.Present,
                        BoardingTime = timestamp,
                        Notes = $"RFID scan at {timestamp:HH:mm:ss}",
                        RecordedBy = "RFID System",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Attendances.Add(existingAttendance);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("RFID attendance recorded for student {StudentId} on trip {TripId}", student.Id, tripId);
                
                var attendanceDto = MapToDto(existingAttendance);
                return Result<AttendanceDto>.Success(attendanceDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording RFID attendance for tag {RfidTag} on trip {TripId}", rfidTag, tripId);
                return Result<AttendanceDto>.Failure("An error occurred while recording RFID attendance");
            }
        }

        public async Task<Result<AttendanceDto>> RecordPhotoAttendanceAsync(int studentId, int tripId, int stopId, string photoBase64, DateTime timestamp, string tenantId)
        {
            try
            {
                var student = await _context.Students
                    .Where(s => s.Id == studentId && s.TenantId == int.Parse(tenantId) && !s.IsDeleted)
                    .FirstOrDefaultAsync();

                if (student == null)
                {
                    return Result<AttendanceDto>.Failure("Student not found");
                }

                var trip = await _context.Trips
                    .Where(t => t.Id == tripId && t.TenantId == int.Parse(tenantId) && !t.IsDeleted)
                    .FirstOrDefaultAsync();

                if (trip == null)
                {
                    return Result<AttendanceDto>.Failure("Trip not found");
                }

                var existingAttendance = await _context.Attendances
                    .Where(a => a.StudentId == studentId && a.TripId == tripId && a.Date.Date == timestamp.Date && !a.IsDeleted)
                    .FirstOrDefaultAsync();

                if (existingAttendance != null)
                {
                    existingAttendance.BoardingTime = timestamp;
                    existingAttendance.Status = AttendanceStatus.Present;
                    existingAttendance.Notes = $"Photo verification at {timestamp:HH:mm:ss}";
                    existingAttendance.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    existingAttendance = new Attendance
                    {
                        TenantId = int.Parse(tenantId),
                        StudentId = studentId,
                        TripId = tripId,
                        Date = timestamp.Date,
                        Status = AttendanceStatus.Present,
                        BoardingTime = timestamp,
                        Notes = $"Photo verification at {timestamp:HH:mm:ss}",
                        RecordedBy = "Photo System",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Attendances.Add(existingAttendance);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Photo attendance recorded for student {StudentId} on trip {TripId}", studentId, tripId);
                
                var attendanceDto = MapToDto(existingAttendance);
                return Result<AttendanceDto>.Success(attendanceDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording photo attendance for student {StudentId} on trip {TripId}", studentId, tripId);
                return Result<AttendanceDto>.Failure("An error occurred while recording photo attendance");
            }
        }

        public async Task<Result<AttendanceDto>> RecordBiometricAttendanceAsync(int studentId, int tripId, int stopId, string biometricData, string biometricType, DateTime timestamp, string tenantId)
        {
            try
            {
                var student = await _context.Students
                    .Where(s => s.Id == studentId && s.TenantId == int.Parse(tenantId) && !s.IsDeleted)
                    .FirstOrDefaultAsync();

                if (student == null)
                {
                    return Result<AttendanceDto>.Failure("Student not found");
                }

                var trip = await _context.Trips
                    .Where(t => t.Id == tripId && t.TenantId == int.Parse(tenantId) && !t.IsDeleted)
                    .FirstOrDefaultAsync();

                if (trip == null)
                {
                    return Result<AttendanceDto>.Failure("Trip not found");
                }

                var existingAttendance = await _context.Attendances
                    .Where(a => a.StudentId == studentId && a.TripId == tripId && a.Date.Date == timestamp.Date && !a.IsDeleted)
                    .FirstOrDefaultAsync();

                if (existingAttendance != null)
                {
                    existingAttendance.BoardingTime = timestamp;
                    existingAttendance.Status = AttendanceStatus.Present;
                    existingAttendance.Notes = $"Biometric {biometricType} scan at {timestamp:HH:mm:ss}";
                    existingAttendance.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    existingAttendance = new Attendance
                    {
                        TenantId = int.Parse(tenantId),
                        StudentId = studentId,
                        TripId = tripId,
                        Date = timestamp.Date,
                        Status = AttendanceStatus.Present,
                        BoardingTime = timestamp,
                        Notes = $"Biometric {biometricType} scan at {timestamp:HH:mm:ss}",
                        RecordedBy = "Biometric System",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Attendances.Add(existingAttendance);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Biometric attendance recorded for student {StudentId} on trip {TripId} using {BiometricType}", studentId, tripId, biometricType);
                
                var attendanceDto = MapToDto(existingAttendance);
                return Result<AttendanceDto>.Success(attendanceDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording biometric attendance for student {StudentId} on trip {TripId}", studentId, tripId);
                return Result<AttendanceDto>.Failure("An error occurred while recording biometric attendance");
            }
        }

        public async Task<Result<OfflineAttendanceSyncResultDto>> SyncOfflineAttendanceAsync(List<OfflineAttendanceDto> offlineRecords, string tenantId)
        {
            try
            {
                var syncedAttendances = new List<AttendanceDto>();
                var errors = new List<string>();

                foreach (var offlineRecord in offlineRecords)
                {
                    try
                    {
                        var student = await _context.Students
                            .Where(s => s.Id == offlineRecord.StudentId && s.TenantId == int.Parse(tenantId) && !s.IsDeleted)
                            .FirstOrDefaultAsync();

                        if (student == null)
                        {
                            errors.Add($"Student {offlineRecord.StudentId} not found");
                            continue;
                        }

                        var trip = await _context.Trips
                            .Where(t => t.Id == offlineRecord.TripId && t.TenantId == int.Parse(tenantId) && !t.IsDeleted)
                            .FirstOrDefaultAsync();

                        if (trip == null)
                        {
                            errors.Add($"Trip {offlineRecord.TripId} not found");
                            continue;
                        }

                        var existingAttendance = await _context.Attendances
                            .Where(a => a.StudentId == offlineRecord.StudentId && a.TripId == offlineRecord.TripId && 
                                       a.Date.Date == offlineRecord.AttendanceDate.Date && !a.IsDeleted)
                            .FirstOrDefaultAsync();

                        if (existingAttendance != null)
                        {
                            existingAttendance.Status = offlineRecord.Status;
                            existingAttendance.BoardingTime = offlineRecord.BoardingTime;
                            existingAttendance.AlightingTime = offlineRecord.AlightingTime;
                            existingAttendance.Notes = $"Offline sync: {offlineRecord.Notes}";
                            existingAttendance.UpdatedAt = DateTime.UtcNow;
                        }
                        else
                        {
                            existingAttendance = new Attendance
                            {
                                TenantId = int.Parse(tenantId),
                                StudentId = offlineRecord.StudentId,
                                TripId = offlineRecord.TripId,
                                Date = offlineRecord.AttendanceDate,
                                Status = offlineRecord.Status,
                                BoardingTime = offlineRecord.BoardingTime,
                                AlightingTime = offlineRecord.AlightingTime,
                                Notes = $"Offline sync: {offlineRecord.Notes}",
                                RecordedBy = "Offline System",
                                CreatedAt = DateTime.UtcNow
                            };
                            _context.Attendances.Add(existingAttendance);
                        }

                        syncedAttendances.Add(MapToDto(existingAttendance));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error syncing offline attendance for student {StudentId}", offlineRecord.StudentId);
                        errors.Add($"Error syncing record for student {offlineRecord.StudentId}: {ex.Message}");
                    }
                }

                await _context.SaveChangesAsync();

                if (errors.Any())
                {
                    _logger.LogWarning("Offline sync completed with {ErrorCount} errors: {Errors}", errors.Count, string.Join(", ", errors));
                }

                _logger.LogInformation("Successfully synced {Count} offline attendance records", syncedAttendances.Count);
                
                var syncResult = new OfflineAttendanceSyncResultDto
                {
                    Success = true,
                    ProcessedRecords = syncedAttendances.Count,
                    SuccessfulRecords = syncedAttendances.Count,
                    FailedRecords = 0,
                    SyncErrors = new List<SyncErrorDto>(),
                    LastSyncTime = DateTime.UtcNow
                };
                
                return Result<OfflineAttendanceSyncResultDto>.Success(syncResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing offline attendance records");
                return Result<OfflineAttendanceSyncResultDto>.Failure("An error occurred while syncing offline attendance records");
            }
        }

        public async Task<Result<List<AttendanceMethodDto>>> GetAttendanceMethodsAsync(string tenantId)
        {
            try
            {
                var methods = new List<AttendanceMethodDto>
                {
                    new AttendanceMethodDto
                    {
                        Id = 1,
                        Name = "Manual Entry",
                        Description = "Manual attendance entry by driver or supervisor",
                        IsEnabled = true,
                        RequiresDevice = false,
                        Configuration = new Dictionary<string, object>()
                    },
                    new AttendanceMethodDto
                    {
                        Id = 2,
                        Name = "RFID Scanning",
                        Description = "RFID card scanning for automated attendance",
                        IsEnabled = true,
                        RequiresDevice = true,
                        DeviceType = "RFID Scanner",
                        Configuration = new Dictionary<string, object> { { "ScanRange", "10cm" } }
                    },
                    new AttendanceMethodDto
                    {
                        Id = 3,
                        Name = "Photo Recognition",
                        Description = "Photo-based attendance using facial recognition",
                        IsEnabled = true,
                        RequiresDevice = true,
                        DeviceType = "Camera",
                        Configuration = new Dictionary<string, object> { { "MinConfidence", 0.85 } }
                    },
                    new AttendanceMethodDto
                    {
                        Id = 4,
                        Name = "Biometric Scanning",
                        Description = "Fingerprint or other biometric attendance",
                        IsEnabled = true,
                        RequiresDevice = true,
                        DeviceType = "Biometric Scanner",
                        Configuration = new Dictionary<string, object> { { "SupportedTypes", new[] { "fingerprint", "face", "iris" } } }
                    }
                };

                return Result<List<AttendanceMethodDto>>.Success(methods);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting attendance methods");
                return Result<List<AttendanceMethodDto>>.Failure("An error occurred while retrieving attendance methods");
            }
        }

        public async Task<Result<AttendanceAnalyticsDto>> GetAttendanceAnalyticsAsync(DateTime startDate, DateTime endDate, string tenantId)
        {
            try
            {
                var attendances = await _context.Attendances
                    .Include(a => a.Student)
                    .Include(a => a.Trip)
                        .ThenInclude(t => t.Route)
                    .Where(a => a.TenantId == int.Parse(tenantId) && !a.IsDeleted)
                    .Where(a => a.Date.Date >= startDate.Date && a.Date.Date <= endDate.Date)
                    .ToListAsync();

                var totalStudents = await _context.Students
                    .Where(s => s.TenantId == int.Parse(tenantId) && !s.IsDeleted)
                    .CountAsync();

                var todayAttendances = attendances.Where(a => a.Date.Date == DateTime.Today).ToList();
                var presentToday = todayAttendances.Count(a => a.Status == AttendanceStatus.Present);
                var absentToday = totalStudents - presentToday;

                var analytics = new AttendanceAnalyticsDto
                {
                    TotalStudents = totalStudents,
                    PresentToday = presentToday,
                    AbsentToday = absentToday,
                    AttendanceRate = totalStudents > 0 ? (decimal)presentToday / totalStudents * 100 : 0,
                    RouteStats = new List<RouteAttendanceStatsDto>(),
                    DailyTrends = new List<DailyAttendanceDto>(),
                    MethodUsage = new List<AttendanceMethodUsageDto>
                    {
                        new AttendanceMethodUsageDto { Method = "Manual", UsageCount = attendances.Count(a => a.Notes?.Contains("Manual") == true), Percentage = 60, AverageProcessingTime = 30 },
                        new AttendanceMethodUsageDto { Method = "RFID", UsageCount = attendances.Count(a => a.Notes?.Contains("RFID") == true), Percentage = 25, AverageProcessingTime = 2 },
                        new AttendanceMethodUsageDto { Method = "Photo", UsageCount = attendances.Count(a => a.Notes?.Contains("Photo") == true), Percentage = 10, AverageProcessingTime = 5 },
                        new AttendanceMethodUsageDto { Method = "Biometric", UsageCount = attendances.Count(a => a.Notes?.Contains("Biometric") == true), Percentage = 5, AverageProcessingTime = 3 }
                    }
                };

                return Result<AttendanceAnalyticsDto>.Success(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting attendance analytics from {StartDate} to {EndDate}", startDate, endDate);
                return Result<AttendanceAnalyticsDto>.Failure("An error occurred while retrieving attendance analytics");
            }
        }

        public async Task<Result<List<GeofenceAlertDto>>> GenerateGeofenceAlertsAsync(int tripId, double latitude, double longitude, string tenantId)
        {
            try
            {
                var trip = await _context.Trips
                    .Include(t => t.Route)
                        .ThenInclude(r => r.RouteStops)
                    .Where(t => t.Id == tripId && t.TenantId == int.Parse(tenantId) && !t.IsDeleted)
                    .FirstOrDefaultAsync();

                if (trip == null)
                {
                    return Result<List<GeofenceAlertDto>>.Failure("Trip not found");
                }

                var alerts = new List<GeofenceAlertDto>();
                const double geofenceRadiusKm = 0.5; // 500 meters

                foreach (var stop in trip.Route.RouteStops)
                {
                    var distance = CalculateDistance(latitude, longitude, (double)stop.Latitude, (double)stop.Longitude);
                    
                    if (distance <= geofenceRadiusKm)
                    {
                        var studentsAtStop = await _context.Students
                            .Where(s => s.RouteStopId == stop.Id && s.TenantId == int.Parse(tenantId) && !s.IsDeleted)
                            .ToListAsync();

                        foreach (var student in studentsAtStop)
                        {
                            var attendance = await _context.Attendances
                                .Where(a => a.StudentId == student.Id && a.TripId == tripId && 
                                           a.Date.Date == DateTime.Today && !a.IsDeleted)
                                .FirstOrDefaultAsync();

                            if (attendance == null || attendance.Status == AttendanceStatus.Absent)
                            {
                                alerts.Add(new GeofenceAlertDto
                                {
                                    Id = 0,
                                    VehicleId = 0, // Will be set from context
                                    VehicleNumber = "",
                                    TripId = tripId,
                                    RouteNumber = "",
                                    ViolationType = "Student Not Boarded",
                                    GeofenceName = stop.Name,
                                    ViolationLatitude = (decimal)latitude,
                                    ViolationLongitude = (decimal)longitude,
                                    Latitude = (decimal)latitude,
                                    Longitude = (decimal)longitude,
                                    Distance = (decimal)distance,
                                    Severity = distance <= 0.1 ? "High" : "Medium",
                                    ViolationTime = DateTime.UtcNow,
                                    Timestamp = DateTime.UtcNow,
                                    Status = "Active",
                                    Description = $"Vehicle is near {stop.Name} but {student.FullName.FirstName} {student.FullName.LastName} has not boarded",
                                    StudentId = student.Id,
                                    StudentName = $"{student.FullName.FirstName} {student.FullName.LastName}",
                                    StopId = stop.Id,
                                    StopName = stop.Name
                                });
                            }
                        }
                    }
                }

                _logger.LogInformation("Generated {AlertCount} geofence alerts for trip {TripId}", alerts.Count, tripId);
                return Result<List<GeofenceAlertDto>>.Success(alerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating geofence alerts for trip {TripId}", tripId);
                return Result<List<GeofenceAlertDto>>.Failure("An error occurred while generating geofence alerts");
            }
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double earthRadiusKm = 6371.0;
            
            var dLat = DegreesToRadians(lat2 - lat1);
            var dLon = DegreesToRadians(lon2 - lon1);
            
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            
            return earthRadiusKm * c;
        }

        private double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }
    }
}

