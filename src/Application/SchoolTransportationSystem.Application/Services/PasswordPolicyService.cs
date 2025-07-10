using System.ComponentModel.DataAnnotations;

namespace SchoolTransportationSystem.Application.Services
{
    public class PasswordPolicyService
    {
        public ValidationResult ValidatePassword(string password, string? email = null)
        {
            var errors = new List<string>();
            
            if (password.Length < 12)
                errors.Add("Password must be at least 12 characters long");
            
            if (!password.Any(char.IsUpper))
                errors.Add("Password must contain at least one uppercase letter");
            
            if (!password.Any(char.IsLower))
                errors.Add("Password must contain at least one lowercase letter");
            
            if (!password.Any(char.IsDigit))
                errors.Add("Password must contain at least one number");
            
            if (!password.Any(c => "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(c)))
                errors.Add("Password must contain at least one special character");
            
            return new ValidationResult { IsValid = !errors.Any(), Errors = errors };
        }
        
        public PasswordStrength GetPasswordStrength(string password)
        {
            var score = 0;
            
            if (password.Length >= 12) score++;
            if (password.Any(char.IsUpper)) score++;
            if (password.Any(char.IsLower)) score++;
            if (password.Any(char.IsDigit)) score++;
            if (password.Any(c => "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(c))) score++;
            
            return score switch
            {
                5 => PasswordStrength.Strong,
                3 or 4 => PasswordStrength.Medium,
                _ => PasswordStrength.Weak
            };
        }
    }
    
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
    }
    
    public enum PasswordStrength
    {
        Weak,
        Medium,
        Strong
    }
}
