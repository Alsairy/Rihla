using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolTransportationSystem.Application.DTOs;
using SchoolTransportationSystem.Application.Interfaces;
using SchoolTransportationSystem.Core.Common;
using SchoolTransportationSystem.Core.Entities;
using SchoolTransportationSystem.Core.Enums;
using SchoolTransportationSystem.Core.ValueObjects;
using SchoolTransportationSystem.Infrastructure.Data;
using System.Linq.Expressions;
using BCrypt.Net;
using System.Security.Cryptography;

namespace SchoolTransportationSystem.Application.Services
{
    public class StudentService : IStudentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StudentService> _logger;
        private readonly IEmailService _emailService;

        public StudentService(ApplicationDbContext context, ILogger<StudentService> logger, IEmailService emailService)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
        }

        // Basic CRUD Operations
        public async Task<Result<StudentDto>> GetByIdAsync(int id, string tenantId)
        {
            try
            {
                var student = await _context.Students
                    .Include(s => s.Route)
                    .Where(s => s.Id == id && s.TenantId == int.Parse(tenantId) && !s.IsDeleted)
                    .FirstOrDefaultAsync();

                if (student == null)
                {
                    return Result<StudentDto>.Failure("Student not found");
                }

                var studentDto = MapToDto(student);
                return Result<StudentDto>.Success(studentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting student by ID {StudentId}", id);
                return Result<StudentDto>.Failure("An error occurred while retrieving the student");
            }
        }

        public async Task<Result<StudentDto>> GetByStudentNumberAsync(string studentNumber, string tenantId)
        {
            try
            {
                var student = await _context.Students
                    .Include(s => s.Route)
                    .Where(s => s.StudentNumber == studentNumber && s.TenantId == int.Parse(tenantId) && !s.IsDeleted)
                    .FirstOrDefaultAsync();

                if (student == null)
                {
                    return Result<StudentDto>.Failure("Student not found");
                }

                var studentDto = MapToDto(student);
                return Result<StudentDto>.Success(studentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting student by number {StudentNumber}", studentNumber);
                return Result<StudentDto>.Failure("An error occurred while retrieving the student");
            }
        }

        public async Task<Result<PagedResult<StudentDto>>> GetAllAsync(StudentSearchDto searchDto, string tenantId)
        {
            try
            {
                var query = _context.Students
                    .Include(s => s.Route)
                    .Where(s => s.TenantId == int.Parse(tenantId) && !s.IsDeleted)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(searchDto.SearchTerm))
                {
                    query = query.Where(s => 
                        s.FullName.FirstName.Contains(searchDto.SearchTerm) ||
                        s.FullName.LastName.Contains(searchDto.SearchTerm) ||
                        s.StudentNumber.Contains(searchDto.SearchTerm) ||
                        s.School.Contains(searchDto.SearchTerm));
                }

                if (!string.IsNullOrEmpty(searchDto.Grade))
                {
                    query = query.Where(s => s.Grade == searchDto.Grade);
                }

                if (!string.IsNullOrEmpty(searchDto.School))
                {
                    query = query.Where(s => s.School == searchDto.School);
                }

                if (searchDto.Status.HasValue)
                {
                    query = query.Where(s => s.Status == searchDto.Status.Value);
                }

                if (searchDto.RouteId.HasValue)
                {
                    query = query.Where(s => s.RouteId == searchDto.RouteId.Value);
                }

                // Apply sorting
                query = searchDto.SortBy?.ToLower() switch
                {
                    "name" => searchDto.SortDescending ? 
                        query.OrderByDescending(s => s.FullName.LastName).ThenByDescending(s => s.FullName.FirstName) :
                        query.OrderBy(s => s.FullName.LastName).ThenBy(s => s.FullName.FirstName),
                    "studentnumber" => searchDto.SortDescending ? 
                        query.OrderByDescending(s => s.StudentNumber) : 
                        query.OrderBy(s => s.StudentNumber),
                    "grade" => searchDto.SortDescending ? 
                        query.OrderByDescending(s => s.Grade) : 
                        query.OrderBy(s => s.Grade),
                    "school" => searchDto.SortDescending ? 
                        query.OrderByDescending(s => s.School) : 
                        query.OrderBy(s => s.School),
                    "status" => searchDto.SortDescending ? 
                        query.OrderByDescending(s => s.Status) : 
                        query.OrderBy(s => s.Status),
                    _ => query.OrderBy(s => s.FullName.LastName).ThenBy(s => s.FullName.FirstName)
                };

                var totalCount = await query.CountAsync();
                var students = await query
                    .Skip((searchDto.Page - 1) * searchDto.PageSize)
                    .Take(searchDto.PageSize)
                    .ToListAsync();

                var studentDtos = students.Select(MapToDto).ToList();

                var pagedResult = new PagedResult<StudentDto>
                {
                    Items = studentDtos,
                    TotalCount = totalCount,
                    Page = searchDto.Page,
                    PageSize = searchDto.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / searchDto.PageSize)
                };

                return Result<PagedResult<StudentDto>>.Success(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting students with search criteria");
                return Result<PagedResult<StudentDto>>.Failure("An error occurred while retrieving students");
            }
        }

        public async Task<Result<StudentDto>> CreateAsync(CreateStudentDto createDto, string tenantId)
        {
            var useTransaction = _context.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory";
            var transaction = useTransaction ? await _context.Database.BeginTransactionAsync() : null;
            
            try
            {
                // Check if student number already exists
                var existingStudent = await _context.Students
                    .Where(s => s.StudentNumber == createDto.StudentNumber && s.TenantId == int.Parse(tenantId))
                    .FirstOrDefaultAsync();

                if (existingStudent != null)
                {
                    return Result<StudentDto>.Failure("Student number already exists");
                }

                User? parentUser = null;
                if (!string.IsNullOrEmpty(createDto.ParentEmail))
                {
                    // Check if parent account already exists
                    var existingParent = await _context.Users
                        .FirstOrDefaultAsync(u => u.Email == createDto.ParentEmail && u.TenantId == tenantId);

                    if (existingParent == null)
                    {
                        var tempPassword = Guid.NewGuid().ToString("N")[..12] + "!A1";
                        
                        var passwordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword);

                        var nameParts = createDto.ParentName?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                        var firstName = nameParts.Length > 0 ? nameParts[0] : "";
                        var lastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "";

                        parentUser = new User
                        {
                            Username = createDto.ParentEmail,
                            Email = createDto.ParentEmail,
                            PasswordHash = passwordHash,
                            Salt = "", // Will be properly implemented in step 003
                            Role = "Parent",
                            TenantId = tenantId,
                            FirstName = firstName,
                            LastName = lastName,
                            IsActive = false, // Requires activation
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.Users.Add(parentUser);
                        await _context.SaveChangesAsync();

                        try
                        {
                            await _emailService.SendParentAccountCreatedAsync(
                                createDto.ParentEmail,
                                createDto.ParentName ?? "",
                                $"{createDto.FirstName} {createDto.LastName}",
                                tempPassword);
                        }
                        catch (Exception emailEx)
                        {
                            _logger.LogWarning(emailEx, "Failed to send parent welcome email to {Email}. Parent account created successfully but email notification failed.", createDto.ParentEmail);
                        }

                        _logger.LogInformation("Parent account created for email {Email} with temporary password", createDto.ParentEmail);
                    }
                    else
                    {
                        parentUser = existingParent;
                    }
                }

                var student = new Student
                {
                    TenantId = int.Parse(tenantId),
                    StudentNumber = createDto.StudentNumber,
                    FullName = new FullName(createDto.FirstName, createDto.LastName, createDto.MiddleName),
                    DateOfBirth = createDto.DateOfBirth,
                    Grade = createDto.Grade,
                    School = createDto.School,
                    Address = new Address(createDto.Street, createDto.City, createDto.State, createDto.ZipCode, createDto.Country),
                    Phone = createDto.Phone,
                    Email = createDto.Email,
                    ParentName = createDto.ParentName,
                    ParentPhone = createDto.ParentPhone,
                    ParentEmail = createDto.ParentEmail,
                    ParentId = parentUser?.Id, // Link to parent User account
                    EmergencyContact = createDto.EmergencyContact,
                    EmergencyPhone = createDto.EmergencyPhone,
                    Status = createDto.Status,
                    EnrollmentDate = createDto.EnrollmentDate,
                    SpecialNeeds = createDto.SpecialNeeds,
                    MedicalConditions = createDto.MedicalConditions,
                    Allergies = createDto.Allergies,
                    Notes = createDto.Notes,
                    RouteId = createDto.RouteId,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                _context.Students.Add(student);
                await _context.SaveChangesAsync();
                
                if (transaction != null)
                {
                    await transaction.CommitAsync();
                }

                var studentDto = MapToDto(student);
                _logger.LogInformation("Student created successfully with ID {StudentId}. Parent account created: {ParentCreated}", 
                    student.Id, parentUser != null);
                
                return Result<StudentDto>.Success(studentDto);
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync();
                }
                _logger.LogError(ex, "Error creating student: {ErrorMessage}", ex.Message);
                return Result<StudentDto>.Failure($"An error occurred while creating the student: {ex.Message}");
            }
            finally
            {
                transaction?.Dispose();
            }
        }

        public async Task<Result<StudentDto>> UpdateAsync(int id, UpdateStudentDto updateDto, string tenantId)
        {
            try
            {
                var student = await _context.Students
                    .Where(s => s.Id == id && s.TenantId == int.Parse(tenantId) && !s.IsDeleted)
                    .FirstOrDefaultAsync();

                if (student == null)
                {
                    return Result<StudentDto>.Failure("Student not found");
                }

                // Check if student number already exists (excluding current student)
                if (updateDto.StudentNumber != student.StudentNumber)
                {
                    var existingStudent = await _context.Students
                        .Where(s => s.StudentNumber == updateDto.StudentNumber && s.TenantId == int.Parse(tenantId) && s.Id != id)
                        .FirstOrDefaultAsync();

                    if (existingStudent != null)
                    {
                        return Result<StudentDto>.Failure("Student number already exists");
                    }
                }

                // Update student properties
                student.StudentNumber = updateDto.StudentNumber;
                student.FullName = new FullName(updateDto.FirstName, updateDto.LastName, updateDto.MiddleName);
                student.DateOfBirth = updateDto.DateOfBirth;
                student.Grade = updateDto.Grade;
                student.School = updateDto.School;
                student.Address = new Address(updateDto.Street, updateDto.City, updateDto.State, updateDto.ZipCode, updateDto.Country);
                student.Phone = updateDto.Phone;
                student.Email = updateDto.Email;
                student.ParentName = updateDto.ParentName;
                student.ParentPhone = updateDto.ParentPhone;
                student.ParentEmail = updateDto.ParentEmail;
                student.EmergencyContact = updateDto.EmergencyContact;
                student.EmergencyPhone = updateDto.EmergencyPhone;
                student.Status = updateDto.Status;
                student.SpecialNeeds = updateDto.SpecialNeeds;
                student.MedicalConditions = updateDto.MedicalConditions;
                student.Allergies = updateDto.Allergies;
                student.Notes = updateDto.Notes;
                student.RouteId = updateDto.RouteId;
                student.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var studentDto = MapToDto(student);
                _logger.LogInformation("Student updated successfully with ID {StudentId}", student.Id);
                
                return Result<StudentDto>.Success(studentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating student with ID {StudentId}", id);
                return Result<StudentDto>.Failure("An error occurred while updating the student");
            }
        }

        public async Task<Result<bool>> DeleteAsync(int id, string tenantId)
        {
            try
            {
                var student = await _context.Students
                    .Where(s => s.Id == id && s.TenantId == int.Parse(tenantId) && !s.IsDeleted)
                    .FirstOrDefaultAsync();

                if (student == null)
                {
                    return Result<bool>.Failure("Student not found");
                }

                student.IsDeleted = true;
                student.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Student deleted successfully with ID {StudentId}", student.Id);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting student with ID {StudentId}", id);
                return Result<bool>.Failure("An error occurred while deleting the student");
            }
        }

        // Additional methods implementation
        public async Task<Result<List<StudentDto>>> GetStudentsByRouteAsync(int routeId, string tenantId)
        {
            try
            {
                var students = await _context.Students
                    .Include(s => s.Route)
                    .Where(s => s.RouteId == routeId && s.TenantId == int.Parse(tenantId) && !s.IsDeleted)
                    .OrderBy(s => s.FullName.LastName)
                    .ThenBy(s => s.FullName.FirstName)
                    .ToListAsync();

                var studentDtos = students.Select(MapToDto).ToList();
                return Result<List<StudentDto>>.Success(studentDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting students by route {RouteId}", routeId);
                return Result<List<StudentDto>>.Failure("An error occurred while retrieving students by route");
            }
        }

        public async Task<Result<List<StudentDto>>> GetStudentsBySchoolAsync(string schoolName, string tenantId)
        {
            try
            {
                var students = await _context.Students
                    .Include(s => s.Route)
                    .Where(s => s.School == schoolName && s.TenantId == int.Parse(tenantId) && !s.IsDeleted)
                    .OrderBy(s => s.FullName.LastName)
                    .ThenBy(s => s.FullName.FirstName)
                    .ToListAsync();

                var studentDtos = students.Select(MapToDto).ToList();
                return Result<List<StudentDto>>.Success(studentDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting students by school {SchoolName}", schoolName);
                return Result<List<StudentDto>>.Failure("An error occurred while retrieving students by school");
            }
        }

        public async Task<Result<List<StudentDto>>> GetStudentsByGradeAsync(string grade, string tenantId)
        {
            try
            {
                var students = await _context.Students
                    .Include(s => s.Route)
                    .Where(s => s.Grade == grade && s.TenantId == int.Parse(tenantId) && !s.IsDeleted)
                    .OrderBy(s => s.FullName.LastName)
                    .ThenBy(s => s.FullName.FirstName)
                    .ToListAsync();

                var studentDtos = students.Select(MapToDto).ToList();
                return Result<List<StudentDto>>.Success(studentDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting students by grade {Grade}", grade);
                return Result<List<StudentDto>>.Failure("An error occurred while retrieving students by grade");
            }
        }

        public async Task<Result<List<StudentTransportationHistoryDto>>> GetTransportationHistoryAsync(int studentId, string tenantId)
        {
            try
            {
                var history = await _context.Attendances
                    .Include(a => a.Trip)
                        .ThenInclude(t => t.Route)
                    .Include(a => a.Trip)
                        .ThenInclude(t => t.Vehicle)
                    .Include(a => a.Trip)
                        .ThenInclude(t => t.Driver)
                    .Where(a => a.StudentId == studentId && a.TenantId == int.Parse(tenantId))
                    .OrderByDescending(a => a.Date)
                    .Take(100) // Limit to last 100 records
                    .ToListAsync();

                var historyDtos = history.Select(a => new StudentTransportationHistoryDto
                {
                    Date = a.Date,
                    TripType = a.Trip.Status.ToString(),
                    RouteName = a.Trip.Route?.Name ?? "Unknown",
                    VehicleNumber = a.Trip.Vehicle?.VehicleNumber ?? "Unknown",
                    DriverName = a.Trip.Driver?.FullName.ToString() ?? "Unknown",
                    PickupTime = a.BoardingTime,
                    DropoffTime = a.AlightingTime,
                    Status = a.Status.ToString(),
                    Notes = a.Notes
                }).ToList();

                return Result<List<StudentTransportationHistoryDto>>.Success(historyDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transportation history for student {StudentId}", studentId);
                return Result<List<StudentTransportationHistoryDto>>.Failure("An error occurred while retrieving transportation history");
            }
        }

        public async Task<Result<List<SchoolStatisticsDto>>> GetSchoolStatisticsAsync(string tenantId)
        {
            try
            {
                var statistics = await _context.Students
                    .Where(s => s.TenantId == int.Parse(tenantId) && !s.IsDeleted)
                    .GroupBy(s => s.School)
                    .Select(g => new SchoolStatisticsDto
                    {
                        SchoolName = g.Key,
                        TotalStudents = g.Count(),
                        ActiveStudents = g.Count(s => s.Status == StudentStatus.Active),
                        InactiveStudents = g.Count(s => s.Status == StudentStatus.Inactive),
                        StudentsWithRoutes = g.Count(s => s.RouteId.HasValue),
                        StudentsWithoutRoutes = g.Count(s => !s.RouteId.HasValue)
                    })
                    .OrderBy(s => s.SchoolName)
                    .ToListAsync();

                return Result<List<SchoolStatisticsDto>>.Success(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting school statistics");
                return Result<List<SchoolStatisticsDto>>.Failure("An error occurred while retrieving school statistics");
            }
        }

        public async Task<Result<bool>> AssignToRouteAsync(int studentId, int routeId, string tenantId)
        {
            try
            {
                var student = await _context.Students
                    .Where(s => s.Id == studentId && s.TenantId == int.Parse(tenantId) && !s.IsDeleted)
                    .FirstOrDefaultAsync();

                if (student == null)
                {
                    return Result<bool>.Failure("Student not found");
                }

                var route = await _context.Routes
                    .Where(r => r.Id == routeId && r.TenantId == int.Parse(tenantId) && !r.IsDeleted)
                    .FirstOrDefaultAsync();

                if (route == null)
                {
                    return Result<bool>.Failure("Route not found");
                }

                student.RouteId = routeId;
                student.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Student {StudentId} assigned to route {RouteId}", studentId, routeId);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning student {StudentId} to route {RouteId}", studentId, routeId);
                return Result<bool>.Failure("An error occurred while assigning student to route");
            }
        }

        public async Task<Result<bool>> RemoveFromRouteAsync(int studentId, string tenantId)
        {
            try
            {
                var student = await _context.Students
                    .Where(s => s.Id == studentId && s.TenantId == int.Parse(tenantId) && !s.IsDeleted)
                    .FirstOrDefaultAsync();

                if (student == null)
                {
                    return Result<bool>.Failure("Student not found");
                }

                student.RouteId = null;
                student.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Student {StudentId} removed from route", studentId);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing student {StudentId} from route", studentId);
                return Result<bool>.Failure("An error occurred while removing student from route");
            }
        }

        public async Task<Result<bool>> BulkUpdateStatusAsync(List<int> studentIds, StudentStatus status, string tenantId)
        {
            try
            {
                var students = await _context.Students
                    .Where(s => studentIds.Contains(s.Id) && s.TenantId == int.Parse(tenantId) && !s.IsDeleted)
                    .ToListAsync();

                foreach (var student in students)
                {
                    student.Status = status;
                    student.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Bulk updated status for {Count} students", students.Count);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk updating student status");
                return Result<bool>.Failure("An error occurred while updating student status");
            }
        }

        // Helper method to map Student entity to StudentDto
        private StudentDto MapToDto(Student student)
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
                School = student.School,
                Street = student.Address.Street,
                City = student.Address.City,
                State = student.Address.State,
                ZipCode = student.Address.ZipCode,
                Country = student.Address.Country,
                Phone = student.Phone,
                Email = student.Email,
                ParentName = student.ParentName,
                ParentPhone = student.ParentPhone,
                ParentEmail = student.ParentEmail,
                EmergencyContact = student.EmergencyContact,
                EmergencyPhone = student.EmergencyPhone,
                Status = student.Status,
                EnrollmentDate = student.EnrollmentDate,
                SpecialNeeds = student.SpecialNeeds,
                MedicalConditions = student.MedicalConditions,
                Allergies = student.Allergies,
                Notes = student.Notes,
                RouteId = student.RouteId,
                RouteName = student.Route?.Name,
                CreatedAt = student.CreatedAt,
                UpdatedAt = student.UpdatedAt
            };
        }

        // Helper methods for parent account creation
        private string GenerateTemporaryPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private (string hash, string salt) HashPassword(string password)
        {
            using var rng = RandomNumberGenerator.Create();
            var saltBytes = new byte[32];
            rng.GetBytes(saltBytes);
            var salt = Convert.ToBase64String(saltBytes);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 10000, HashAlgorithmName.SHA256);
            var hash = Convert.ToBase64String(pbkdf2.GetBytes(32));

            return (hash, salt);
        }

        private string ExtractFirstName(string fullName)
        {
            var parts = fullName?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            return parts.Length > 0 ? parts[0] : "";
        }

        private string ExtractLastName(string fullName)
        {
            var parts = fullName?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            return parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : "";
        }

        private async Task SendParentWelcomeEmail(User parentUser, string tempPassword, CreateStudentDto studentDto)
        {
            var subject = "Welcome to Rihla Transportation System - Account Activation Required";
            var body = $@"
                <h2>Welcome to Rihla Transportation System</h2>
                <p>Dear {parentUser.FirstName} {parentUser.LastName},</p>
                <p>A parent account has been created for you to access your child's transportation information.</p>
                
                <h3>Student Information:</h3>
                <p><strong>Student:</strong> {studentDto.FirstName} {studentDto.LastName}</p>
                <p><strong>Grade:</strong> {studentDto.Grade}</p>
                <p><strong>School:</strong> {studentDto.School}</p>
                
                <h3>Account Details:</h3>
                <p><strong>Email:</strong> {parentUser.Email}</p>
                <p><strong>Temporary Password:</strong> {tempPassword}</p>
                
                <p><strong>Next Steps:</strong></p>
                <ol>
                    <li>Visit the parent portal at: <a href=""http://localhost:3000/parent"">Parent Portal</a></li>
                    <li>Log in with your email and temporary password</li>
                    <li>You will be prompted to change your password on first login</li>
                    <li>Complete your profile setup</li>
                </ol>
                
                <p>If you have any questions, please contact the school administration.</p>
                <p>Best regards,<br>Rihla Transportation Team</p>
            ";

            _logger.LogInformation("Parent welcome email prepared for {Email}. Email service integration pending.", parentUser.Email);
        }
    }
}

