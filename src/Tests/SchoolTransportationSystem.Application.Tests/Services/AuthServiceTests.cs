using Xunit;
using FluentAssertions;
using SchoolTransportationSystem.Application.Services;
using SchoolTransportationSystem.Application.DTOs;
using SchoolTransportationSystem.Core.Entities;
using SchoolTransportationSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SchoolTransportationSystem.Application.Tests.Services
{
    public class AuthServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly UserService _userService;
        private const string TestTenantId = "1";

        public AuthServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _userService = new UserService(_context);
        }

        [Fact]
        public async Task AuthenticateAsync_ReturnsSuccess_WhenValidCredentials()
        {
            var (passwordHash, salt) = HashPassword("password123");
            var user = new User
            {
                Id = 1,
                TenantId = TestTenantId,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = passwordHash,
                Salt = salt,
                Role = "SuperAdmin",
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = await _userService.AuthenticateAsync("test@example.com", "password123", TestTenantId);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Username.Should().Be("testuser");
            result.Value.Email.Should().Be("test@example.com");
        }

        [Fact]
        public async Task AuthenticateAsync_ReturnsFailure_WhenInvalidEmail()
        {
            var result = await _userService.AuthenticateAsync("nonexistent@example.com", "password123", TestTenantId);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("Invalid email or password");
        }

        [Fact]
        public async Task AuthenticateAsync_ReturnsFailure_WhenInvalidPassword()
        {
            var (passwordHash, salt) = HashPassword("admin123");
            var user = new User
            {
                Id = 1,
                TenantId = TestTenantId,
                Username = "testuser",
                Email = "admin@rihla.sa",
                PasswordHash = passwordHash,
                Salt = salt,
                Role = "SuperAdmin",
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = await _userService.AuthenticateAsync("admin@rihla.sa", "wrongpassword", TestTenantId);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("Invalid email or password");
        }

        [Fact]
        public async Task AuthenticateAsync_ReturnsFailure_WhenUserInactive()
        {
            var (passwordHash, salt) = HashPassword("password123");
            var user = new User
            {
                Id = 1,
                TenantId = TestTenantId,
                Username = "inactiveuser",
                Email = "inactive@rihla.sa",
                PasswordHash = passwordHash,
                Salt = salt,
                Role = "Driver",
                IsActive = false
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = await _userService.AuthenticateAsync("inactive@rihla.sa", "password123", TestTenantId);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("Invalid email or password");
        }

        [Fact]
        public async Task CreateAsync_ReturnsSuccess_WhenValidRegistration()
        {
            var createUserDto = new CreateUserDto
            {
                Username = "newuser",
                Email = "newuser@example.com",
                Password = "password123",
                FirstName = "New",
                LastName = "User",
                Role = "Parent",
                IsActive = true
            };

            var result = await _userService.CreateAsync(createUserDto, TestTenantId);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Username.Should().Be("newuser");
            result.Value.Email.Should().Be("newuser@example.com");

            var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Username == "newuser");
            userInDb.Should().NotBeNull();
            userInDb.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task CreateAsync_ReturnsFailure_WhenUsernameAlreadyExists()
        {
            var (passwordHash, salt) = HashPassword("password123");
            var existingUser = new User
            {
                Id = 1,
                TenantId = TestTenantId,
                Username = "existinguser",
                Email = "existing@example.com",
                PasswordHash = passwordHash,
                Salt = salt,
                Role = "Parent",
                IsActive = true
            };

            _context.Users.Add(existingUser);
            await _context.SaveChangesAsync();

            var createUserDto = new CreateUserDto
            {
                Username = "existinguser", // Duplicate username
                Email = "different@example.com",
                Password = "password123",
                FirstName = "Different",
                LastName = "User",
                Role = "Parent",
                IsActive = true
            };

            var result = await _userService.CreateAsync(createUserDto, TestTenantId);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("User with this email or username already exists");
        }

        private (string hash, string salt) HashPassword(string password)
        {
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            var saltBytes = new byte[32];
            rng.GetBytes(saltBytes);
            var salt = Convert.ToBase64String(saltBytes);

            using var pbkdf2 = new System.Security.Cryptography.Rfc2898DeriveBytes(password, saltBytes, 10000, System.Security.Cryptography.HashAlgorithmName.SHA256);
            var hash = Convert.ToBase64String(pbkdf2.GetBytes(32));

            return (hash, salt);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
