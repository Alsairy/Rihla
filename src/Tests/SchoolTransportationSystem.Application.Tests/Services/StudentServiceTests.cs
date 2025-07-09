using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Rihla.Application.Services;
using Rihla.Application.Interfaces;
using Rihla.Core.Entities;
using Rihla.Core.Interfaces;
using Rihla.Application.DTOs;
using Rihla.Core.Common;
using Rihla.Core.Enums;
using Rihla.Core.ValueObjects;
using System.Linq.Expressions;
using MockQueryable.Moq;

namespace SchoolTransportationSystem.Application.Tests.Services
{
    public class StudentServiceTests
    {
        private readonly Mock<IRepository<Student>> _mockStudentRepository;
        private readonly Mock<IRepository<Route>> _mockRouteRepository;
        private readonly Mock<IRepository<Attendance>> _mockAttendanceRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<StudentService>> _mockLogger;
        private readonly StudentService _studentService;

        public StudentServiceTests()
        {
            _mockStudentRepository = new Mock<IRepository<Student>>();
            _mockRouteRepository = new Mock<IRepository<Route>>();
            _mockAttendanceRepository = new Mock<IRepository<Attendance>>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<StudentService>>();

            _mockUnitOfWork.Setup(u => u.Students).Returns(_mockStudentRepository.Object);
            _mockUnitOfWork.Setup(u => u.Routes).Returns(_mockRouteRepository.Object);
            _mockUnitOfWork.Setup(u => u.Attendances).Returns(_mockAttendanceRepository.Object);

            _studentService = new StudentService(
                _mockUnitOfWork.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsStudent_WhenStudentExists()
        {
            var student = CreateTestStudent();
            var students = new List<Student> { student }.AsQueryable().BuildMock();
            
            _mockStudentRepository.Setup(r => r.QueryWithIncludes("1", It.IsAny<Expression<Func<Student, object>>[]>()))
                .Returns(students);

            var result = await _studentService.GetByIdAsync(1, "1");

            Assert.True(result.IsSuccess, $"Expected success but got error: {result.Error}");
            Assert.Equal(1, result.Value.Id);
            Assert.Equal("John", result.Value.FirstName);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsFailure_WhenStudentNotFound()
        {
            var students = new List<Student>().AsQueryable().BuildMock();
            _mockStudentRepository.Setup(r => r.QueryWithIncludes("1", It.IsAny<Expression<Func<Student, object>>[]>()))
                .Returns(students);

            var result = await _studentService.GetByIdAsync(999, "1");

            Assert.False(result.IsSuccess);
            Assert.Equal("Student not found", result.Error);
        }

        [Fact]
        public async Task CreateAsync_ReturnsSuccess_WhenValidStudent()
        {
            var createDto = new CreateStudentDto
            {
                StudentNumber = "STU001",
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = DateTime.Now.AddYears(-10),
                Grade = "5",
                School = "Test School",
                Street = "123 Main St",
                City = "Test City",
                State = "Test State",
                ZipCode = "12345",
                Country = "Test Country",
                ParentName = "Parent Name",
                ParentPhone = "123-456-7890",
                EnrollmentDate = DateTime.Now,
                Status = StudentStatus.Active
            };

            var emptyStudents = new List<Student>().AsQueryable().BuildMock();
            _mockStudentRepository.Setup(r => r.Query("1"))
                .Returns(emptyStudents);
            
            var createdStudent = CreateTestStudent();
            _mockStudentRepository.Setup(r => r.AddAsync(It.IsAny<Student>()))
                .Returns(Task.FromResult(createdStudent));
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            var result = await _studentService.CreateAsync(createDto, "1");

            Assert.True(result.IsSuccess, $"Expected success but got error: {result.Error}");
            _mockStudentRepository.Verify(r => r.AddAsync(It.IsAny<Student>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsSuccess_WhenStudentExists()
        {
            var student = CreateTestStudent();
            
            _mockStudentRepository.Setup(r => r.GetByIdAsync(1, "1"))
                .ReturnsAsync(student);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            var result = await _studentService.DeleteAsync(1, "1");

            Assert.True(result.IsSuccess);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFailure_WhenStudentNotFound()
        {
            _mockStudentRepository.Setup(r => r.GetByIdAsync(999, "1"))
                .ReturnsAsync((Student?)null);

            var result = await _studentService.DeleteAsync(999, "1");

            Assert.False(result.IsSuccess);
            Assert.Equal("Student not found", result.Error);
        }

        private Student CreateTestStudent()
        {
            return new Student
            {
                Id = 1,
                TenantId = 1,
                StudentNumber = "STU001",
                FullName = new FullName("John", "Doe", null),
                DateOfBirth = DateTime.Now.AddYears(-10),
                Grade = "5",
                School = "Test School",
                Address = new Address("123 Main St", "Test City", "Test State", "12345", "Test Country"),
                ParentName = "Parent Name",
                ParentPhone = "123-456-7890",
                Status = StudentStatus.Active,
                EnrollmentDate = DateTime.Now,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
        }

        [Fact]
        public async Task CreateAsync_ReturnsFailure_WhenStudentNumberExists()
        {
            var createDto = new CreateStudentDto
            {
                StudentNumber = "STU001",
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = DateTime.Now.AddYears(-10),
                Grade = "5",
                School = "Test School",
                Street = "123 Main St",
                City = "Test City",
                State = "Test State",
                ZipCode = "12345",
                Country = "Test Country",
                ParentName = "Parent Name",
                ParentPhone = "123-456-7890",
                EnrollmentDate = DateTime.Now,
                Status = StudentStatus.Active
            };

            var existingStudent = CreateTestStudent();
            var studentsWithExisting = new List<Student> { existingStudent }.AsQueryable().BuildMock();
            _mockStudentRepository.Setup(r => r.Query("1"))
                .Returns(studentsWithExisting);

            var result = await _studentService.CreateAsync(createDto, "1");

            Assert.False(result.IsSuccess);
            Assert.Equal("Student number already exists", result.Error);
        }
    }
}
