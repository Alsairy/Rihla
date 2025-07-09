using Xunit;
using Moq;
using FluentAssertions;
using SchoolTransportationSystem.Application.Services;
using SchoolTransportationSystem.Application.DTOs;
using SchoolTransportationSystem.Core.Entities;
using SchoolTransportationSystem.Core.ValueObjects;
using SchoolTransportationSystem.Core.Enums;
using SchoolTransportationSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SchoolTransportationSystem.Application.Tests.Services
{
    public class StudentServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<ILogger<StudentService>> _mockLogger;
        private readonly StudentService _studentService;
        private const string TestTenantId = "1";

        public StudentServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _mockLogger = new Mock<ILogger<StudentService>>();
            _studentService = new StudentService(_context, _mockLogger.Object);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsStudent_WhenStudentExists()
        {
            var student = new Student
            {
                Id = 1,
                TenantId = 1,
                StudentNumber = "STU001",
                FullName = new FullName("John", "Doe", null),
                Email = "john.doe@example.com",
                Grade = "10",
                School = "Test School",
                Address = new Address("123 Main St", "Test City", "Test State", "12345", "Test Country"),
                Phone = "123-456-7890",
                DateOfBirth = DateTime.Now.AddYears(-16),
                Status = StudentStatus.Active,
                EnrollmentDate = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            var result = await _studentService.GetByIdAsync(1, TestTenantId);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Id.Should().Be(1);
            result.Value.FirstName.Should().Be("John");
            result.Value.LastName.Should().Be("Doe");
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsFailure_WhenStudentNotFound()
        {
            var result = await _studentService.GetByIdAsync(999, TestTenantId);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("not found");
        }

        [Fact]
        public async Task CreateAsync_ReturnsSuccess_WhenValidStudent()
        {
            var createStudentDto = new CreateStudentDto
            {
                StudentNumber = "STU002",
                FirstName = "Fatima",
                LastName = "Al-Zahra",
                Email = "fatima.zahra@example.com",
                Phone = "+966501234569",
                Street = "123 Main St",
                City = "Jeddah",
                State = "Makkah",
                ZipCode = "21577",
                Country = "Saudi Arabia",
                ParentName = "Ali Al-Zahra",
                ParentPhone = "+966501234570",
                RouteId = 2,
                Grade = "9th Grade",
                School = "King Fahd Academy",
                DateOfBirth = DateTime.Now.AddYears(-15),
                EnrollmentDate = DateTime.UtcNow
            };

            var result = await _studentService.CreateAsync(createStudentDto, TestTenantId);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.FirstName.Should().Be("Fatima");
            result.Value.LastName.Should().Be("Al-Zahra");
            result.Value.Email.Should().Be("fatima.zahra@example.com");

            var studentInDb = await _context.Students.FirstOrDefaultAsync(s => s.StudentNumber == "STU002");
            studentInDb.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateAsync_ReturnsSuccess_WhenNoRouteSpecified()
        {
            var createStudentDto = new CreateStudentDto
            {
                StudentNumber = "STU003",
                FirstName = "Test",
                LastName = "Student",
                Email = "test@example.com",
                Phone = "+966501234567",
                Street = "Test Street",
                City = "Test City",
                State = "Test State",
                ZipCode = "12345",
                Country = "Test Country",
                ParentName = "Test Parent",
                ParentPhone = "+966501234568",
                Grade = "10th Grade",
                School = "Test School",
                DateOfBirth = DateTime.Now.AddYears(-16),
                EnrollmentDate = DateTime.UtcNow
            };

            var result = await _studentService.CreateAsync(createStudentDto, TestTenantId);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.FirstName.Should().Be("Test");
            result.Value.LastName.Should().Be("Student");

            var studentInDb = await _context.Students.FirstOrDefaultAsync(s => s.StudentNumber == "STU003");
            studentInDb.Should().NotBeNull();
            studentInDb.RouteId.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_ReturnsSuccess_WhenValidUpdate()
        {
            var student = new Student
            {
                Id = 1,
                TenantId = 1,
                StudentNumber = "STU004",
                FullName = new FullName("Ahmed", "Al-Rashid", null),
                Email = "ahmed.rashid@example.com",
                Phone = "+966501234567",
                Address = new Address("123 Main St", "Riyadh", "Riyadh", "11564", "Saudi Arabia"),
                ParentName = "Mohammed Al-Rashid",
                ParentPhone = "+966501234568",
                RouteId = 1,
                Status = StudentStatus.Active,
                Grade = "10th Grade",
                School = "Al-Noor International School",
                DateOfBirth = DateTime.Now.AddYears(-16),
                EnrollmentDate = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            var updateStudentDto = new UpdateStudentDto
            {
                StudentNumber = "STU004",
                FirstName = "Ahmed Updated",
                LastName = "Al-Rashid Updated",
                Email = "ahmed.updated@example.com",
                Phone = "+966501234567",
                Street = "456 Updated St",
                City = "Riyadh",
                State = "Riyadh",
                ZipCode = "11564",
                Country = "Saudi Arabia",
                ParentName = "Mohammed Al-Rashid",
                ParentPhone = "+966501234568",
                RouteId = 1,
                Grade = "11th Grade",
                School = "Al-Noor International School",
                DateOfBirth = DateTime.Now.AddYears(-16),
                Status = StudentStatus.Active
            };

            var result = await _studentService.UpdateAsync(1, updateStudentDto, TestTenantId);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.FirstName.Should().Be("Ahmed Updated");
            result.Value.Grade.Should().Be("11th Grade");
        }

        [Fact]
        public async Task DeleteAsync_ReturnsSuccess_WhenStudentExists()
        {
            var student = new Student
            {
                Id = 1,
                TenantId = 1,
                StudentNumber = "STU005",
                FullName = new FullName("Ahmed", "Al-Rashid", null),
                Email = "ahmed.rashid@example.com",
                Grade = "10th Grade",
                School = "Test School",
                Address = new Address("123 Main St", "Test City", "Test State", "12345", "Test Country"),
                Phone = "123-456-7890",
                DateOfBirth = DateTime.Now.AddYears(-16),
                Status = StudentStatus.Active,
                EnrollmentDate = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            var result = await _studentService.DeleteAsync(1, TestTenantId);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();

            var deletedStudent = await _context.Students.FindAsync(1);
            deletedStudent.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllActiveStudents()
        {
            var students = new List<Student>
            {
                new Student 
                { 
                    Id = 1, 
                    TenantId = 1,
                    StudentNumber = "STU006",
                    FullName = new FullName("Ahmed", "Al-Rashid", null), 
                    Email = "ahmed@example.com", 
                    Grade = "10",
                    School = "Test School",
                    Address = new Address("123 Main St", "Test City", "Test State", "12345", "Test Country"),
                    Phone = "123-456-7890",
                    DateOfBirth = DateTime.Now.AddYears(-16),
                    Status = StudentStatus.Active,
                    EnrollmentDate = DateTime.UtcNow,
                    IsDeleted = false
                },
                new Student 
                { 
                    Id = 2, 
                    TenantId = 1,
                    StudentNumber = "STU007",
                    FullName = new FullName("Fatima", "Al-Zahra", null), 
                    Email = "fatima@example.com", 
                    Grade = "11",
                    School = "Test School",
                    Address = new Address("456 Oak Ave", "Test City", "Test State", "12345", "Test Country"),
                    Phone = "987-654-3210",
                    DateOfBirth = DateTime.Now.AddYears(-17),
                    Status = StudentStatus.Active,
                    EnrollmentDate = DateTime.UtcNow,
                    IsDeleted = false
                }
            };

            _context.Students.AddRange(students);
            await _context.SaveChangesAsync();

            var searchDto = new StudentSearchDto();
            var result = await _studentService.GetAllAsync(searchDto, TestTenantId);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Items.Should().HaveCount(2);
            result.Value.Items.Should().OnlyContain(s => s.Status == StudentStatus.Active);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
