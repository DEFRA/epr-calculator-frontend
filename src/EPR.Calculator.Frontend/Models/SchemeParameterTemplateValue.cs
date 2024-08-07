using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.Models
{
    [ExcludeFromCodeCoverage]
    public class SchemeParameterTemplateValue
    {
        public string ParameterUniqueReferenceId { get; set; }

        public decimal? ParameterValue { get; set; }
    }
}
