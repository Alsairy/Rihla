using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SchoolTransportationSystem.WebAPI.Controllers;
using SchoolTransportationSystem.Application.Interfaces;
using SchoolTransportationSystem.Application.DTOs;
using SchoolTransportationSystem.Core.Common;
using SchoolTransportationSystem.Core.Entities;

namespace SchoolTransportationSystem.WebAPI.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<AuthController>> _mockLogger;
        private readonly Mock<IUserService> _mockUserService;
        private readonly AuthController _controller;
        private const string TestTenantId = "1";

        public AuthControllerTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<AuthController>>();
            _mockUserService = new Mock<IUserService>();
            
            var jwtSection = new Mock<IConfigurationSection>();
            jwtSection.Setup(x => x["SecretKey"]).Returns("your-super-secret-key-that-is-at-least-32-characters-long");
            jwtSection.Setup(x => x["Issuer"]).Returns("Rihla");
            jwtSection.Setup(x => x["Audience"]).Returns("RihlaUsers");
            _mockConfiguration.Setup(x => x.GetSection("JwtSettings")).Returns(jwtSection.Object);
            
            _controller = new AuthController(_mockConfiguration.Object, _mockLogger.Object, _mockUserService.Object);
        }

        [Fact]
        public async Task Login_ReturnsOk_WhenValidCredentials()
        {
            var loginDto = new LoginDto
            {
                Email = "admin@rihla.sa",
                Password = "admin123"
            };

            var user = new User
            {
                Id = 1,
                Email = "admin@rihla.sa",
                FirstName = "Admin",
                LastName = "User",
                Role = "Admin",
                Username = "admin",
                TenantId = TestTenantId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };

            _mockUserService.Setup(service => service.AuthenticateAsync(loginDto.Email, loginDto.Password, TestTenantId))
                .ReturnsAsync(Result<User>.Success(user));

            _mockUserService.Setup(service => service.UpdateRefreshTokenAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>(), TestTenantId))
                .ReturnsAsync(Result<bool>.Success(true));

            var result = await _controller.Login(loginDto);

            var actionResult = result.Should().BeOfType<ActionResult<object>>().Subject;
            var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().NotBeNull();

            _mockUserService.Verify(service => service.AuthenticateAsync(loginDto.Email, loginDto.Password, TestTenantId), Times.Once);
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenInvalidCredentials()
        {
            var loginDto = new LoginDto
            {
                Email = "invalid@example.com",
                Password = "wrongpassword"
            };

            _mockUserService.Setup(service => service.AuthenticateAsync(loginDto.Email, loginDto.Password, TestTenantId))
                .ReturnsAsync(Result<User>.Failure("Invalid credentials"));

            var result = await _controller.Login(loginDto);

            var actionResult = result.Should().BeOfType<ActionResult<object>>().Subject;
            var unauthorizedResult = actionResult.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
            unauthorizedResult.Value.Should().NotBeNull();

            _mockUserService.Verify(service => service.AuthenticateAsync(loginDto.Email, loginDto.Password, TestTenantId), Times.Once);
        }

        [Fact]
        public async Task Login_ReturnsBadRequest_WhenModelStateInvalid()
        {
            var loginDto = new LoginDto
            {
                Email = "", // Invalid email
                Password = "" // Invalid password
            };

            _controller.ModelState.AddModelError("Email", "Email is required");
            _controller.ModelState.AddModelError("Password", "Password is required");

            var result = await _controller.Login(loginDto);

            var actionResult = result.Should().BeOfType<ActionResult<object>>().Subject;
            actionResult.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task RefreshToken_ReturnsOk_WhenValidRefreshToken()
        {
            var refreshDto = new RefreshTokenDto
            {
                RefreshToken = "valid-refresh-token"
            };

            var user = new User
            {
                Id = 1,
                Email = "admin@rihla.sa",
                FirstName = "Admin",
                LastName = "User",
                Role = "Admin",
                Username = "admin",
                TenantId = TestTenantId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };

            _mockUserService.Setup(service => service.ValidateRefreshTokenAsync(refreshDto.RefreshToken, TestTenantId))
                .ReturnsAsync(Result<User>.Success(user));

            _mockUserService.Setup(service => service.UpdateRefreshTokenAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>(), TestTenantId))
                .ReturnsAsync(Result<bool>.Success(true));

            var result = await _controller.RefreshToken(refreshDto);

            var actionResult = result.Should().BeOfType<ActionResult<object>>().Subject;
            var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().NotBeNull();

            _mockUserService.Verify(service => service.ValidateRefreshTokenAsync(refreshDto.RefreshToken, TestTenantId), Times.Once);
        }

        [Fact]
        public async Task RefreshToken_ReturnsUnauthorized_WhenInvalidRefreshToken()
        {
            var refreshDto = new RefreshTokenDto
            {
                RefreshToken = "invalid-refresh-token"
            };

            _mockUserService.Setup(service => service.ValidateRefreshTokenAsync(refreshDto.RefreshToken, TestTenantId))
                .ReturnsAsync(Result<User>.Failure("Invalid refresh token"));

            var result = await _controller.RefreshToken(refreshDto);

            var actionResult = result.Should().BeOfType<ActionResult<object>>().Subject;
            var unauthorizedResult = actionResult.Result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
            unauthorizedResult.Value.Should().NotBeNull();

            _mockUserService.Verify(service => service.ValidateRefreshTokenAsync(refreshDto.RefreshToken, TestTenantId), Times.Once);
        }
    }
}
