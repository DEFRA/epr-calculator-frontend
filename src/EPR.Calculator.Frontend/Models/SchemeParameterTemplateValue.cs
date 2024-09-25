using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.Models
{
    [ExcludeFromCodeCoverage]
    public class SchemeParameterTemplateValue
    {
        public required string ParameterUniqueReferenceId { get; set; }

        public required string ParameterValue { get; set; }
    }
}
