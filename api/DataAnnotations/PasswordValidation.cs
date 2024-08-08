using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace api.DataAnnotations 
{
    public class PasswordValidation : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? password, ValidationContext validationContext)
        {
            var passwordString = password != null ? password.ToString() : null;

            if (string.IsNullOrWhiteSpace(passwordString) || passwordString.Length < 6) return new ValidationResult("Пароль должен содержать не менее 6 символов");

            if (!passwordString.Any(char.IsDigit)) return new ValidationResult("Пароль должен содержать хотя бы одну цифру");

            if (!Regex.IsMatch(passwordString, @"[!@#$%^&*()_+}{'?/><|~`]")) return new ValidationResult("Пароль должен содержать хотя бы один специальный символ");

            if (!passwordString.Any(ch => char.IsLower(ch) && ch >= 'a' && ch <= 'z')) return new ValidationResult("Пароль должен содержать хотя бы одну строчную букву ('a'-'z')");

            if (!passwordString.Any(ch => char.IsUpper(ch) && ch >= 'A' && ch <= 'Z')) return new ValidationResult("Пароль должен содержать хотя бы одну заглавную букву ('A'-'Z')");

            return ValidationResult.Success;
        }
    }
}
