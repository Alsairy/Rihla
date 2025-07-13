using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolTransportationSystem.Infrastructure.Data;
using SchoolTransportationSystem.Core.Entities;
using SchoolTransportationSystem.Application.DTOs;
using SchoolTransportationSystem.Core.ValueObjects;
using SchoolTransportationSystem.Core.Enums;
using SchoolTransportationSystem.Application.Interfaces;

namespace SchoolTransportationSystem.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StudentsController> _logger;
        private readonly IStudentService _studentService;

        public StudentsController(ApplicationDbContext context, ILogger<StudentsController> logger, IStudentService studentService)
        {
            _context = context;
            _logger = logger;
            _studentService = studentService;
        }

        [HttpGet]
        public async Task<ActionResult<List<StudentDto>>> GetStudents()
        {
            try
            {
                var students = await _context.Students
                    .Where(s => !s.IsDeleted)
                    .Select(s => new StudentDto
                    {
                        Id = s.Id,
                        TenantId = s.TenantId,
                        StudentNumber = s.StudentNumber,
                        FirstName = s.FullName.FirstName,
                        LastName = s.FullName.LastName,
                        MiddleName = s.FullName.MiddleName,
                        DateOfBirth = s.DateOfBirth,
                        Grade = s.Grade,
                        School = s.School,
                        Street = s.Address.Street,
                        City = s.Address.City,
                        State = s.Address.State,
                        ZipCode = s.Address.ZipCode,
                        Country = s.Address.Country,
                        Phone = s.Phone,
                        Email = s.Email,
                        ParentName = s.ParentName ?? "",
                        ParentPhone = s.ParentPhone ?? "",
                        ParentEmail = s.ParentEmail,
                        EmergencyContact = s.EmergencyContact,
                        EmergencyPhone = s.EmergencyPhone,
                        Status = s.Status,
                        EnrollmentDate = s.EnrollmentDate,
                        SpecialNeeds = s.SpecialNeeds,
                        MedicalConditions = s.MedicalConditions,
                        Allergies = s.Allergies,
                        Notes = s.Notes,
                        RouteId = s.RouteId,
                        CreatedAt = s.CreatedAt,
                        UpdatedAt = s.UpdatedAt
                    })
                    .ToListAsync();

                return Ok(students);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving students");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StudentDto>> GetStudent(int id)
        {
            try
            {
                var student = await _context.Students
                    .Where(s => s.Id == id && !s.IsDeleted)
                    .Select(s => new StudentDto
                    {
                        Id = s.Id,
                        TenantId = s.TenantId,
                        StudentNumber = s.StudentNumber,
                        FirstName = s.FullName.FirstName,
                        LastName = s.FullName.LastName,
                        MiddleName = s.FullName.MiddleName,
                        DateOfBirth = s.DateOfBirth,
                        Grade = s.Grade,
                        School = s.School,
                        Street = s.Address.Street,
                        City = s.Address.City,
                        State = s.Address.State,
                        ZipCode = s.Address.ZipCode,
                        Country = s.Address.Country,
                        Phone = s.Phone,
                        Email = s.Email,
                        ParentName = s.ParentName ?? "",
                        ParentPhone = s.ParentPhone ?? "",
                        ParentEmail = s.ParentEmail,
                        EmergencyContact = s.EmergencyContact,
                        EmergencyPhone = s.EmergencyPhone,
                        Status = s.Status,
                        EnrollmentDate = s.EnrollmentDate,
                        SpecialNeeds = s.SpecialNeeds,
                        MedicalConditions = s.MedicalConditions,
                        Allergies = s.Allergies,
                        Notes = s.Notes,
                        RouteId = s.RouteId,
                        CreatedAt = s.CreatedAt,
                        UpdatedAt = s.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                if (student == null)
                {
                    return NotFound();
                }

                return Ok(student);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving student {StudentId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<ActionResult<StudentDto>> CreateStudent(CreateStudentDto createDto)
        {
            try
            {
                var tenantId = "1"; // Use default tenant ID for now
                var result = await _studentService.CreateAsync(createDto, tenantId);

                if (!result.IsSuccess)
                {
                    return BadRequest(result.Error);
                }

                return CreatedAtAction(nameof(GetStudent), new { id = result.Value.Id }, result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating student");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudent(int id, UpdateStudentDto updateDto)
        {
            try
            {
                var student = await _context.Students
                    .Where(s => s.Id == id && !s.IsDeleted)
                    .FirstOrDefaultAsync();

                if (student == null)
                {
                    return NotFound();
                }

                student.FullName = new FullName(updateDto.FirstName, updateDto.LastName, updateDto.MiddleName);
                student.DateOfBirth = updateDto.DateOfBirth;
                student.Grade = updateDto.Grade;
                student.School = updateDto.School;
                student.Address = new Address(updateDto.Street, updateDto.City, updateDto.State, updateDto.ZipCode, updateDto.Country);
                student.Phone = updateDto.Phone ?? "";
                student.Email = updateDto.Email ?? "";
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
                student.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating student {StudentId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            try
            {
                var student = await _context.Students
                    .Where(s => s.Id == id && !s.IsDeleted)
                    .FirstOrDefaultAsync();

                if (student == null)
                {
                    return NotFound();
                }

                student.IsDeleted = true;
                student.DeletedAt = DateTime.UtcNow;
                student.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting student {StudentId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("my-children")]
        public async Task<ActionResult<List<StudentDto>>> GetMyChildren()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                if (!int.TryParse(userId, out int parentIdInt))
                    return BadRequest("Invalid user ID format");

                var students = await _context.Students
                    .Where(s => !s.IsDeleted && s.ParentId == parentIdInt)
                    .Select(s => new StudentDto
                    {
                        Id = s.Id,
                        TenantId = s.TenantId,
                        StudentNumber = s.StudentNumber,
                        FirstName = s.FullName.FirstName,
                        LastName = s.FullName.LastName,
                        MiddleName = s.FullName.MiddleName,
                        DateOfBirth = s.DateOfBirth,
                        Grade = s.Grade,
                        School = s.School,
                        Street = s.Address.Street,
                        City = s.Address.City,
                        State = s.Address.State,
                        ZipCode = s.Address.ZipCode,
                        Country = s.Address.Country,
                        Phone = s.Phone,
                        Email = s.Email,
                        ParentName = s.ParentName ?? "",
                        ParentPhone = s.ParentPhone ?? "",
                        ParentEmail = s.ParentEmail,
                        EmergencyContact = s.EmergencyContact,
                        EmergencyPhone = s.EmergencyPhone,
                        Status = s.Status,
                        EnrollmentDate = s.EnrollmentDate,
                        SpecialNeeds = s.SpecialNeeds,
                        MedicalConditions = s.MedicalConditions,
                        Allergies = s.Allergies,
                        Notes = s.Notes,
                        RouteId = s.RouteId,
                        CreatedAt = s.CreatedAt,
                        UpdatedAt = s.UpdatedAt
                    })
                    .ToListAsync();

                return Ok(students);
            }
            catch (Exception ex)
            {
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                _logger.LogError(ex, "Error retrieving children for parent {UserId}", currentUserId);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}

