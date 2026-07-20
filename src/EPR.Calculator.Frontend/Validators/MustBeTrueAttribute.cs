using System.ComponentModel.DataAnnotations;

namespace EPR.Calculator.Frontend.Validators;

public class MustBeTrueAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is true)
            return ValidationResult.Success;

        return new ValidationResult(ErrorMessage ?? "The field must be checked.");
    }
}
