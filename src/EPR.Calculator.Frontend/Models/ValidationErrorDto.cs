using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.Models
{
    [ExcludeFromCodeCoverage]
    public class ValidationErrorDto
    {
        public string? Exception { get; set; }

        public required string ErrorMessage { get; set; }
    }
}
