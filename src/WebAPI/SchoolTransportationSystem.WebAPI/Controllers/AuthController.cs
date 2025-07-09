using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SchoolTransportationSystem.Application.Interfaces;
using SchoolTransportationSystem.Application.DTOs;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;

namespace SchoolTransportationSystem.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly IUserService _userService;

        public AuthController(IConfiguration configuration, ILogger<AuthController> logger, IUserService userService)
        {
            _configuration = configuration;
            _logger = logger;
            _userService = userService;
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

                var tenantId = "1"; // Default tenant for now
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
                var authResult = await _userService.AuthenticateAsync(loginDto.Email, loginDto.Password, tenantId, ipAddress, userAgent);
                
                if (!authResult.IsSuccess)
                {
                    return Unauthorized(new { message = "Invalid email or password" });
                }

                var user = authResult.Value;
                var token = GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken();

                await _userService.UpdateRefreshTokenAsync(user.Id, refreshToken, DateTime.UtcNow.AddDays(7), tenantId);

                var response = new LoginResponseDto
                {
                    User = new UserDto
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        Role = user.Role,
                        TenantId = user.TenantId,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        IsActive = user.IsActive,
                        CreatedAt = user.CreatedAt,
                        LastLoginAt = user.LastLoginAt
                    },
                    Token = token,
                    RefreshToken = refreshToken,
                    TokenExpiry = DateTime.UtcNow.AddHours(24)
                };

                return Ok(new { success = true, data = response, message = "Login successful" });
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
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    var tenantId = "1"; // Default tenant for now
                    await _userService.RevokeRefreshTokenAsync(userId, tenantId);
                }

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
                var tenantId = "1"; // Default tenant for now
                var validationResult = await _userService.ValidateRefreshTokenAsync(refreshDto.RefreshToken, tenantId);
                
                if (!validationResult.IsSuccess)
                {
                    return Unauthorized(new { message = "Invalid or expired refresh token" });
                }

                var user = validationResult.Value;
                var newToken = GenerateJwtToken(user);
                var newRefreshToken = GenerateRefreshToken();

                await _userService.UpdateRefreshTokenAsync(user.Id, newRefreshToken, DateTime.UtcNow.AddDays(7), tenantId);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        token = newToken,
                        refreshToken = newRefreshToken,
                        tokenExpiry = DateTime.UtcNow.AddHours(24)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        private string GenerateJwtToken(SchoolTransportationSystem.Core.Entities.User user)
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
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("tenant_id", user.TenantId),
                new Claim("username", user.Username)
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

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        [HttpPost("setup-mfa")]
        [Authorize]
        public async Task<ActionResult<object>> SetupMfa()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                return Ok(new 
                { 
                    success = true, 
                    message = "MFA setup endpoint ready - implementation pending MfaService",
                    data = new 
                    {
                        qrCodeUrl = "placeholder-qr-code-url",
                        secret = "placeholder-secret",
                        backupCodes = new string[] { "backup1", "backup2", "backup3" }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting up MFA");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("verify-mfa")]
        public async Task<ActionResult<object>> VerifyMfa([FromBody] VerifyMfaDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(new 
                { 
                    success = true, 
                    message = "MFA verification endpoint ready - implementation pending MfaService",
                    data = new 
                    {
                        verified = false,
                        token = "placeholder-token"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying MFA");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("disable-mfa")]
        [Authorize]
        public async Task<ActionResult<object>> DisableMfa([FromBody] DisableMfaDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                return Ok(new 
                { 
                    success = true, 
                    message = "MFA disable endpoint ready - implementation pending MfaService"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disabling MFA");
                return StatusCode(500, new { message = "Internal server error" });
            }
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

    public class VerifyMfaDto
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? TempToken { get; set; }
    }

    public class DisableMfaDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string MfaCode { get; set; } = string.Empty;
    }
}

