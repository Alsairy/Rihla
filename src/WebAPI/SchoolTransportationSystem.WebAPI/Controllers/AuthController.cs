using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Rihla.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult<object>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Mock authentication - replace with real authentication logic
                var user = AuthenticateUser(loginDto.Email, loginDto.Password);
                
                if (user == null)
                {
                    return Unauthorized(new { message = "Invalid email or password" });
                }

                var token = GenerateJwtToken(user);

                var response = new
                {
                    success = true,
                    data = new
                    {
                        user = new
                        {
                            user.Id,
                            user.Email,
                            user.Name,
                            user.Role
                        },
                        token = token
                    },
                    message = "Login successful"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("logout")]
        public async Task<ActionResult<object>> Logout()
        {
            try
            {
                // In a real implementation, you might want to blacklist the token
                return Ok(new { success = true, message = "Logout successful" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<object>> RefreshToken([FromBody] RefreshTokenDto refreshDto)
        {
            try
            {
                // Mock token refresh - implement real refresh logic
                var user = new UserInfo
                {
                    Id = 1,
                    Email = "admin@rihla.sa",
                    Name = "Admin User",
                    Role = "admin"
                };

                var newToken = GenerateJwtToken(user);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        token = newToken
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        private UserInfo? AuthenticateUser(string email, string password)
        {
            // Mock users - replace with database lookup
            var mockUsers = new List<UserInfo>
            {
                new UserInfo { Id = 1, Email = "admin@rihla.sa", Name = "Admin User", Role = "admin", Password = "admin123" },
                new UserInfo { Id = 2, Email = "manager@rihla.sa", Name = "Manager User", Role = "manager", Password = "manager123" },
                new UserInfo { Id = 3, Email = "driver@rihla.sa", Name = "Driver User", Role = "driver", Password = "driver123" },
                new UserInfo { Id = 4, Email = "parent@rihla.sa", Name = "Parent User", Role = "parent", Password = "parent123" }
            };

            return mockUsers.FirstOrDefault(u => 
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && 
                u.Password == password);
        }

        private string GenerateJwtToken(UserInfo user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? "your-super-secret-key-that-is-at-least-32-characters-long";
            var issuer = jwtSettings["Issuer"] ?? "Rihla";
            var audience = jwtSettings["Audience"] ?? "RihlaUsers";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("tenant_id", "1") // Default tenant
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RefreshTokenDto
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class UserInfo
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; // Only for mock authentication
    }
}

