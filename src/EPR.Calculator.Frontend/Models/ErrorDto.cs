using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.Models
{
    [ExcludeFromCodeCoverage]
    public class ErrorDto
    {
        public string Message { get; set; }

        public string Description { get; set; }
    }
}
