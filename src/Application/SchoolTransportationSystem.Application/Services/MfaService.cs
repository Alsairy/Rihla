using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolTransportationSystem.Application.Interfaces;
using SchoolTransportationSystem.Core.Common;
using SchoolTransportationSystem.Core.Entities;
using SchoolTransportationSystem.Infrastructure.Data;

namespace SchoolTransportationSystem.Application.Services
{
    public class MfaService : IMfaService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MfaService> _logger;

        public MfaService(ApplicationDbContext context, ILogger<MfaService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<MfaSetupResult>> SetupMfaAsync(int userId, string tenantId)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

                if (user == null)
                {
                    return Result<MfaSetupResult>.Failure("User not found");
                }

                if (user.MfaEnabled)
                {
                    return Result<MfaSetupResult>.Failure("MFA is already enabled for this user");
                }

                var secret = GenerateSecret();
                var backupCodes = GenerateBackupCodes();
                var qrCodeUrl = GenerateQrCodeUrl(user.Email, secret);

                user.MfaSecret = secret;
                user.BackupCodes = string.Join(",", backupCodes);

                await _context.SaveChangesAsync();

                var result = new MfaSetupResult
                {
                    Secret = secret,
                    QrCodeUrl = qrCodeUrl,
                    BackupCodes = backupCodes
                };

                return Result<MfaSetupResult>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting up MFA for user {UserId}", userId);
                return Result<MfaSetupResult>.Failure($"Error setting up MFA: {ex.Message}");
            }
        }

        public async Task<Result<bool>> VerifyMfaCodeAsync(int userId, string code, string tenantId)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

                if (user == null)
                {
                    return Result<bool>.Failure("User not found");
                }

                if (string.IsNullOrEmpty(user.MfaSecret))
                {
                    return Result<bool>.Failure("MFA is not set up for this user");
                }

                if (!string.IsNullOrEmpty(user.BackupCodes))
                {
                    var backupCodes = user.BackupCodes.Split(',').ToList();
                    if (backupCodes.Contains(code))
                    {
                        backupCodes.Remove(code);
                        user.BackupCodes = string.Join(",", backupCodes);
                        await _context.SaveChangesAsync();
                        return Result<bool>.Success(true);
                    }
                }

                var isValid = VerifyTotpCode(user.MfaSecret, code);
                return Result<bool>.Success(isValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying MFA code for user {UserId}", userId);
                return Result<bool>.Failure($"Error verifying MFA code: {ex.Message}");
            }
        }

        public async Task<Result<bool>> EnableMfaAsync(int userId, string verificationCode, string tenantId)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

                if (user == null)
                {
                    return Result<bool>.Failure("User not found");
                }

                if (string.IsNullOrEmpty(user.MfaSecret))
                {
                    return Result<bool>.Failure("MFA setup is required before enabling");
                }

                var verificationResult = await VerifyMfaCodeAsync(userId, verificationCode, tenantId);
                if (!verificationResult.IsSuccess || !verificationResult.Value)
                {
                    return Result<bool>.Failure("Invalid verification code");
                }

                user.MfaEnabled = true;
                await _context.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enabling MFA for user {UserId}", userId);
                return Result<bool>.Failure($"Error enabling MFA: {ex.Message}");
            }
        }

        public async Task<Result<bool>> DisableMfaAsync(int userId, string verificationCode, string tenantId)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);

                if (user == null)
                {
                    return Result<bool>.Failure("User not found");
                }

                if (!user.MfaEnabled)
                {
                    return Result<bool>.Failure("MFA is not enabled for this user");
                }

                var verificationResult = await VerifyMfaCodeAsync(userId, verificationCode, tenantId);
                if (!verificationResult.IsSuccess || !verificationResult.Value)
                {
                    return Result<bool>.Failure("Invalid verification code");
                }

                user.MfaEnabled = false;
                user.MfaSecret = null;
                user.BackupCodes = null;
                await _context.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disabling MFA for user {UserId}", userId);
                return Result<bool>.Failure($"Error disabling MFA: {ex.Message}");
            }
        }

        private string GenerateSecret()
        {
            var bytes = new byte[20];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes).Replace("=", "").Replace("+", "").Replace("/", "");
        }

        private string[] GenerateBackupCodes()
        {
            var codes = new string[8];
            using var rng = RandomNumberGenerator.Create();
            
            for (int i = 0; i < codes.Length; i++)
            {
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                codes[i] = BitConverter.ToUInt32(bytes, 0).ToString("D8");
            }
            
            return codes;
        }

        private string GenerateQrCodeUrl(string email, string secret)
        {
            var issuer = "Rihla School Transport";
            var accountName = email;
            var otpAuthUrl = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(accountName)}?secret={secret}&issuer={Uri.EscapeDataString(issuer)}";
            return $"https://api.qrserver.com/v1/create-qr-code/?size=200x200&data={Uri.EscapeDataString(otpAuthUrl)}";
        }

        private bool VerifyTotpCode(string secret, string code)
        {
            if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(code) || code.Length != 6)
                return false;

            var secretBytes = Convert.FromBase64String(secret + new string('=', (4 - secret.Length % 4) % 4));
            var unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var timeStep = unixTime / 30;

            for (int i = -1; i <= 1; i++)
            {
                var testTimeStep = timeStep + i;
                var expectedCode = GenerateTotpCode(secretBytes, testTimeStep);
                if (expectedCode == code)
                    return true;
            }

            return false;
        }

        private string GenerateTotpCode(byte[] secret, long timeStep)
        {
            var timeBytes = BitConverter.GetBytes(timeStep);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(timeBytes);

            using var hmac = new HMACSHA1(secret);
            var hash = hmac.ComputeHash(timeBytes);
            
            var offset = hash[hash.Length - 1] & 0x0F;
            var binaryCode = ((hash[offset] & 0x7F) << 24) |
                           ((hash[offset + 1] & 0xFF) << 16) |
                           ((hash[offset + 2] & 0xFF) << 8) |
                           (hash[offset + 3] & 0xFF);
            
            var code = binaryCode % 1000000;
            return code.ToString("D6");
        }
    }

    public class MfaSetupResult
    {
        public string Secret { get; set; } = string.Empty;
        public string QrCodeUrl { get; set; } = string.Empty;
        public string[] BackupCodes { get; set; } = Array.Empty<string>();
    }

    public interface IMfaService
    {
        Task<Result<MfaSetupResult>> SetupMfaAsync(int userId, string tenantId);
        Task<Result<bool>> VerifyMfaCodeAsync(int userId, string code, string tenantId);
        Task<Result<bool>> EnableMfaAsync(int userId, string verificationCode, string tenantId);
        Task<Result<bool>> DisableMfaAsync(int userId, string verificationCode, string tenantId);
    }
}
