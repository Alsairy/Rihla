using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Rihla.Infrastructure.Data;
using Rihla.Core.Entities;
using Rihla.Application.DTOs;
using Rihla.Core.ValueObjects;
using Rihla.Core.Enums;

namespace Rihla.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ManagerOrAbove")]
    public class StudentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StudentsController> _logger;

        public StudentsController(ApplicationDbContext context, ILogger<StudentsController> logger)
        {
            _context = context;
            _logger = logger;
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
                var student = new Student
                {
                    TenantId = 1, // Use default tenant ID for now
                    StudentNumber = createDto.StudentNumber,
                    FullName = new FullName(createDto.FirstName, createDto.LastName, createDto.MiddleName),
                    DateOfBirth = createDto.DateOfBirth,
                    Grade = createDto.Grade,
                    School = createDto.School,
                    Address = new Address(createDto.Street, createDto.City, createDto.State, createDto.ZipCode, createDto.Country),
                    Phone = createDto.Phone ?? "",
                    Email = createDto.Email ?? "",
                    ParentName = createDto.ParentName,
                    ParentPhone = createDto.ParentPhone,
                    ParentEmail = createDto.ParentEmail,
                    EmergencyContact = createDto.EmergencyContact,
                    EmergencyPhone = createDto.EmergencyPhone,
                    Status = createDto.Status,
                    EnrollmentDate = createDto.EnrollmentDate,
                    SpecialNeeds = createDto.SpecialNeeds,
                    MedicalConditions = createDto.MedicalConditions,
                    Allergies = createDto.Allergies,
                    Notes = createDto.Notes,
                    RouteId = createDto.RouteId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Students.Add(student);
                await _context.SaveChangesAsync();

                var studentDto = new StudentDto
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
                    ParentName = student.ParentName ?? "",
                    ParentPhone = student.ParentPhone ?? "",
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
                    CreatedAt = student.CreatedAt,
                    UpdatedAt = student.UpdatedAt
                };

                return CreatedAtAction(nameof(GetStudent), new { id = student.Id }, studentDto);
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
    }
}

