using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.Models
{
    [ExcludeFromCodeCoverage]
    public class ValidationErrorDto
    {
        public string Exception { get; set; }

        public string ErrorMessage { get; set; }
    }
}
