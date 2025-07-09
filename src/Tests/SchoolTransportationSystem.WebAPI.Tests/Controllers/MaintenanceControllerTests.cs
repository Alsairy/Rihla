using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SchoolTransportationSystem.WebAPI.Controllers;
using SchoolTransportationSystem.Application.Interfaces;
using SchoolTransportationSystem.Application.DTOs;
using SchoolTransportationSystem.Core.Common;
using SchoolTransportationSystem.Core.Enums;

namespace SchoolTransportationSystem.WebAPI.Tests.Controllers
{
    public class MaintenanceControllerTests
    {
        private readonly Mock<IMaintenanceService> _mockMaintenanceService;
        private readonly MaintenanceController _controller;
        private const string TestTenantId = "1";

        public MaintenanceControllerTests()
        {
            _mockMaintenanceService = new Mock<IMaintenanceService>();
            _controller = new MaintenanceController(_mockMaintenanceService.Object);
        }

        [Fact]
        public async Task GetMaintenanceRecords_ReturnsOkResult_WhenMaintenanceRecordsExist()
        {
            var searchDto = new MaintenanceSearchDto();
            var maintenanceRecords = new List<MaintenanceRecordDto>
            {
                new MaintenanceRecordDto { Id = 1, VehicleId = 1, Description = "Oil Change", Cost = 150.00m },
                new MaintenanceRecordDto { Id = 2, VehicleId = 2, Description = "Brake Repair", Cost = 300.00m }
            };
            var pagedResult = new PagedResult<MaintenanceRecordDto>
            {
                Items = maintenanceRecords,
                TotalCount = 2,
                Page = 1,
                PageSize = 20
            };

            _mockMaintenanceService
                .Setup(s => s.GetAllAsync(It.IsAny<MaintenanceSearchDto>(), TestTenantId))
                .ReturnsAsync(Result<PagedResult<MaintenanceRecordDto>>.Success(pagedResult));

            var result = await _controller.GetMaintenanceRecords(searchDto);

            var actionResult = result.Should().BeOfType<ActionResult<IEnumerable<MaintenanceRecordDto>>>().Subject;
            var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedData = okResult.Value.Should().BeAssignableTo<PagedResult<MaintenanceRecordDto>>().Subject;
            returnedData.Items.Should().HaveCount(2);
            returnedData.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task GetMaintenanceRecord_ReturnsOkResult_WhenMaintenanceRecordExists()
        {
            var maintenanceId = 1;
            var maintenanceRecord = new MaintenanceRecordDto 
            { 
                Id = maintenanceId, 
                VehicleId = 1, 
                Description = "Oil Change", 
                Cost = 150.00m 
            };

            _mockMaintenanceService
                .Setup(s => s.GetByIdAsync(maintenanceId, TestTenantId))
                .ReturnsAsync(Result<MaintenanceRecordDto>.Success(maintenanceRecord));

            var result = await _controller.GetMaintenanceRecord(maintenanceId);

            var actionResult = result.Should().BeOfType<ActionResult<MaintenanceRecordDto>>().Subject;
            var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedData = okResult.Value.Should().BeAssignableTo<MaintenanceRecordDto>().Subject;
            returnedData.Id.Should().Be(maintenanceId);
            returnedData.Description.Should().Be("Oil Change");
        }

        [Fact]
        public async Task GetMaintenanceRecord_ReturnsNotFound_WhenMaintenanceRecordDoesNotExist()
        {
            var maintenanceId = 999;
            _mockMaintenanceService
                .Setup(s => s.GetByIdAsync(maintenanceId, TestTenantId))
                .ReturnsAsync(Result<MaintenanceRecordDto>.Failure("Maintenance record not found"));

            var result = await _controller.GetMaintenanceRecord(maintenanceId);

            var actionResult = result.Should().BeOfType<ActionResult<MaintenanceRecordDto>>().Subject;
            actionResult.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task CreateMaintenanceRecord_ReturnsCreatedResult_WhenValidMaintenanceRecord()
        {
            var createDto = new CreateMaintenanceRecordDto
            {
                VehicleId = 1,
                Description = "Oil Change",
                Cost = 150.00m,
                ScheduledDate = DateTime.UtcNow,
                Mileage = 50000,
                Type = MaintenanceType.Preventive,
                TenantId = TestTenantId
            };

            var createdMaintenance = new MaintenanceRecordDto
            {
                Id = 1,
                VehicleId = createDto.VehicleId,
                Description = createDto.Description,
                Cost = createDto.Cost
            };

            _mockMaintenanceService
                .Setup(s => s.CreateAsync(createDto, TestTenantId))
                .ReturnsAsync(Result<MaintenanceRecordDto>.Success(createdMaintenance));

            var result = await _controller.CreateMaintenanceRecord(createDto);

            var actionResult = result.Should().BeOfType<ActionResult<MaintenanceRecordDto>>().Subject;
            var createdResult = actionResult.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            var returnedData = createdResult.Value.Should().BeAssignableTo<MaintenanceRecordDto>().Subject;
            returnedData.Id.Should().Be(1);
            returnedData.Description.Should().Be("Oil Change");
        }

        [Fact]
        public async Task UpdateMaintenanceRecord_ReturnsOkResult_WhenValidUpdate()
        {
            var maintenanceId = 1;
            var updateDto = new UpdateMaintenanceRecordDto
            {
                Id = maintenanceId,
                Description = "Updated Oil Change",
                Cost = 175.00m,
                ScheduledDate = DateTime.UtcNow,
                Mileage = 50000,
                Type = MaintenanceType.Preventive,
                Status = MaintenanceStatus.Completed
            };

            var updatedMaintenance = new MaintenanceRecordDto
            {
                Id = maintenanceId,
                VehicleId = 1,
                Description = updateDto.Description,
                Cost = updateDto.Cost,
                Type = updateDto.Type,
                Status = updateDto.Status,
                ScheduledDate = updateDto.ScheduledDate
            };

            _mockMaintenanceService
                .Setup(s => s.UpdateAsync(maintenanceId, updateDto, TestTenantId))
                .ReturnsAsync(Result<MaintenanceRecordDto>.Success(updatedMaintenance));

            var result = await _controller.UpdateMaintenanceRecord(maintenanceId, updateDto);

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task DeleteMaintenanceRecord_ReturnsOkResult_WhenMaintenanceRecordExists()
        {
            var maintenanceId = 1;
            _mockMaintenanceService
                .Setup(s => s.DeleteAsync(maintenanceId, TestTenantId))
                .ReturnsAsync(Result<bool>.Success(true));

            var result = await _controller.DeleteMaintenanceRecord(maintenanceId);

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task DeleteMaintenanceRecord_ReturnsNotFound_WhenMaintenanceRecordDoesNotExist()
        {
            var maintenanceId = 999;
            _mockMaintenanceService
                .Setup(s => s.DeleteAsync(maintenanceId, TestTenantId))
                .ReturnsAsync(Result<bool>.Failure("Maintenance record not found"));

            var result = await _controller.DeleteMaintenanceRecord(maintenanceId);

            result.Should().BeOfType<NotFoundObjectResult>();
        }
    }
}
