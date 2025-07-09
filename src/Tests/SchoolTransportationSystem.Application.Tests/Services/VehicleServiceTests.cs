using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Rihla.Application.Services;
using Rihla.Core.Entities;
using Rihla.Core.Interfaces;
using Rihla.Application.DTOs;
using Rihla.Core.Common;
using Rihla.Core.Enums;

namespace SchoolTransportationSystem.Application.Tests.Services
{
    public class VehicleServiceTests
    {
        private readonly Mock<IRepository<Vehicle>> _mockVehicleRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<VehicleService>> _mockLogger;
        private readonly VehicleService _vehicleService;

        public VehicleServiceTests()
        {
            _mockVehicleRepository = new Mock<IRepository<Vehicle>>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<VehicleService>>();

            _mockUnitOfWork.Setup(u => u.Vehicles).Returns(_mockVehicleRepository.Object);

            _vehicleService = new VehicleService(
                _mockUnitOfWork.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsVehicle_WhenVehicleExists()
        {
            var vehicle = CreateTestVehicle();
            
            _mockVehicleRepository.Setup(r => r.GetByIdAsync(1, "1"))
                .ReturnsAsync(vehicle);

            var result = await _vehicleService.GetByIdAsync(1, "1");

            Assert.True(result.IsSuccess, $"Expected success but got error: {result.Error}");
            Assert.Equal(1, result.Value.Id);
            Assert.Equal("VEH001", result.Value.VehicleNumber);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsSuccess_WhenVehicleExists()
        {
            var vehicle = CreateTestVehicle();
            
            _mockVehicleRepository.Setup(r => r.GetByIdAsync(1, "1"))
                .ReturnsAsync(vehicle);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            var result = await _vehicleService.DeleteAsync(1, "1");

            Assert.True(result.IsSuccess);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        private Vehicle CreateTestVehicle()
        {
            return new Vehicle
            {
                Id = 1,
                TenantId = 1,
                VehicleNumber = "VEH001",
                Make = "Toyota",
                Model = "Hiace",
                Year = 2020,
                LicensePlate = "ABC123",
                Capacity = 15,
                Status = VehicleStatus.Active,
                PurchaseDate = DateTime.Now.AddYears(-2),
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
        }
    }
}
