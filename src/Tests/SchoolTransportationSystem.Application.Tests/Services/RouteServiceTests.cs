using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Rihla.Application.Services;
using Rihla.Core.Entities;
using Rihla.Core.Interfaces;
using Rihla.Application.DTOs;
using Rihla.Core.Common;
using Rihla.Core.Enums;
using MockQueryable.Moq;

namespace SchoolTransportationSystem.Application.Tests.Services
{
    public class RouteServiceTests
    {
        private readonly Mock<IRepository<Route>> _mockRouteRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<RouteService>> _mockLogger;
        private readonly RouteService _routeService;

        public RouteServiceTests()
        {
            _mockRouteRepository = new Mock<IRepository<Route>>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<RouteService>>();

            _mockUnitOfWork.Setup(u => u.Routes).Returns(_mockRouteRepository.Object);

            _routeService = new RouteService(
                _mockUnitOfWork.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsRoute_WhenRouteExists()
        {
            var route = CreateTestRoute();
            var routes = new List<Route> { route }.AsQueryable().BuildMock();
            
            _mockRouteRepository.Setup(r => r.QueryWithIncludes("1", It.IsAny<System.Linq.Expressions.Expression<Func<Route, object>>[]>()))
                .Returns(routes);

            var result = await _routeService.GetByIdAsync(1, "1");

            Assert.True(result.IsSuccess, $"Expected success but got error: {result.Error}");
            Assert.Equal(1, result.Value.Id);
            Assert.Equal("Route 1", result.Value.Name);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsSuccess_WhenRouteExists()
        {
            var route = CreateTestRoute();
            
            _mockRouteRepository.Setup(r => r.GetByIdAsync(1, "1"))
                .ReturnsAsync(route);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
                .ReturnsAsync(1);

            var result = await _routeService.DeleteAsync(1, "1");

            Assert.True(result.IsSuccess);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        private Route CreateTestRoute()
        {
            return new Route
            {
                Id = 1,
                TenantId = 1,
                Name = "Route 1",
                Description = "Test route description",
                StartLocation = "Start Point",
                EndLocation = "End Point",
                Distance = 10.5m,
                EstimatedDuration = 30,
                Status = RouteStatus.Active,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
        }
    }
}
