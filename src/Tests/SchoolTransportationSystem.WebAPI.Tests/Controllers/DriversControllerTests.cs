using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Xunit;
using Rihla.Application.DTOs;
using Rihla.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Rihla.Infrastructure.Data;

namespace SchoolTransportationSystem.WebAPI.Tests.Controllers
{
    public class DriversControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public DriversControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDb");
                    });
                });
            });
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetDrivers_ReturnsSuccessStatusCode()
        {
            var response = await _client.GetAsync("/api/drivers?page=1&pageSize=10");
            
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(content);
        }

        [Fact]
        public async Task CreateDriver_WithValidData_ReturnsCreated()
        {
            var createDto = new CreateDriverDto
            {
                EmployeeNumber = "DRV001",
                FirstName = "Test",
                LastName = "Driver",
                DateOfBirth = DateTime.Now.AddYears(-30),
                Phone = "123-456-7890",
                Email = "test.driver@test.com",
                Street = "123 Test St",
                City = "Test City",
                State = "Test State",
                ZipCode = "12345",
                Country = "Test Country",
                LicenseNumber = "LIC123456",
                LicenseExpiry = DateTime.Now.AddYears(2),
                HireDate = DateTime.Now,
                Status = DriverStatus.Active
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/drivers", content);
            
            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task GetDriver_WithInvalidId_ReturnsNotFound()
        {
            var response = await _client.GetAsync("/api/drivers/99999");
            
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
