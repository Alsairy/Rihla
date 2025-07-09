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
    public class StudentsControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public StudentsControllerTests(WebApplicationFactory<Program> factory)
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
        public async Task GetStudents_ReturnsSuccessStatusCode()
        {
            var response = await _client.GetAsync("/api/students?page=1&pageSize=10");
            
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(content);
        }

        [Fact]
        public async Task CreateStudent_WithValidData_ReturnsCreated()
        {
            var createDto = new CreateStudentDto
            {
                StudentNumber = "TEST001",
                FirstName = "Test",
                LastName = "Student",
                DateOfBirth = DateTime.Now.AddYears(-10),
                Grade = "5",
                School = "Test School",
                Street = "123 Test St",
                City = "Test City",
                State = "Test State",
                ZipCode = "12345",
                Country = "Test Country",
                ParentName = "Test Parent",
                ParentPhone = "123-456-7890",
                EnrollmentDate = DateTime.Now,
                Status = StudentStatus.Active
            };

            var json = JsonSerializer.Serialize(createDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/students", content);
            
            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task GetStudent_WithInvalidId_ReturnsNotFound()
        {
            var response = await _client.GetAsync("/api/students/99999");
            
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
