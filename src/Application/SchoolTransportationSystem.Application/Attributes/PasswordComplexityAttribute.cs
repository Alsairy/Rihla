using System.ComponentModel.DataAnnotations;
using SchoolTransportationSystem.Application.Services;

namespace SchoolTransportationSystem.Application.Attributes
{
    public class PasswordComplexityAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is not string password)
                return false;

            var passwordPolicyService = new PasswordPolicyService();
            var result = passwordPolicyService.ValidatePassword(password);
            
            if (!result.IsValid)
            {
                ErrorMessage = string.Join("; ", result.Errors);
                return false;
            }

            return true;
        }
    }
}
