using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Rihla.Application.Services;
using Rihla.Core.Entities;
using Rihla.Core.Interfaces;
using Rihla.Application.DTOs;
using Rihla.Core.Common;
using Rihla.Core.Enums;
using Rihla.Core.ValueObjects;

namespace SchoolTransportationSystem.Application.Tests.Services
{
    public class DriverServiceTests
    {
        private readonly Mock<IRepository<Driver>> _mockDriverRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<DriverService>> _mockLogger;
        private readonly DriverService _driverService;

        public DriverServiceTests()
        {
            _mockDriverRepository = new Mock<IRepository<Driver>>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<DriverService>>();

            _mockUnitOfWork.Setup(u => u.Drivers).Returns(_mockDriverRepository.Object);

            _driverService = new DriverService(
                _mockUnitOfWork.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsDriver_WhenDriverExists()
        {
            var driver = CreateTestDriver();
            
            _mockDriverRepository.Setup(r => r.GetByIdAsync(1, "1"))
                .ReturnsAsync(driver);

            var result = await _driverService.GetByIdAsync(1, "1");

            Assert.True(result.IsSuccess, $"Expected success but got error: {result.Error}");
            Assert.Equal(1, result.Value.Id);
            Assert.Equal("John", result.Value.FirstName);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsSuccess_WhenDriverExists()
        {
            var driver = CreateTestDriver();
            
            _mockDriverRepository.Setup(r => r.GetByIdAsync(1, "1"))
                .ReturnsAsync(driver);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            var result = await _driverService.DeleteAsync(1, "1");

            Assert.True(result.IsSuccess);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        private Driver CreateTestDriver()
        {
            return new Driver
            {
                Id = 1,
                TenantId = 1,
                EmployeeNumber = "DRV001",
                FullName = new FullName("John", "Doe", null),
                DateOfBirth = DateTime.Now.AddYears(-30),
                Phone = "123-456-7890",
                Email = "john.doe@test.com",
                Address = new Address("123 Main St", "Test City", "Test State", "12345", "Test Country"),
                LicenseNumber = "LIC123456",
                LicenseExpiry = DateTime.Now.AddYears(2),
                HireDate = DateTime.Now.AddYears(-1),
                Status = DriverStatus.Active,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
        }
    }
}
