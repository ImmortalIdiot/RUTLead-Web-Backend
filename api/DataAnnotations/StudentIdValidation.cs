using System.ComponentModel.DataAnnotations;

namespace api.DataAnnotations
{
    public class StudentIdValidation : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? studentId, ValidationContext validationContext)
        {
            var studentIdString = studentId != null ? studentId.ToString() : null;

            if (string.IsNullOrWhiteSpace(studentIdString) || studentIdString.Length != 8) return new ValidationResult("Номер студенческого билета должен содержать ровно 8 символов");

            return ValidationResult.Success;
        }
    }
}
