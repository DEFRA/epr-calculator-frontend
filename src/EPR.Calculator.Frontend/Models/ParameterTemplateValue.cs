using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.Models
{
    [ExcludeFromCodeCoverage]
    public class ParameterTemplateValue
    {
        public required string ParameterUniqueRef { get; set; }

        public required string ParameterType { get; set; }

        public required string ParameterCategory { get; set; }

        public required string ValidRangeFrom { get; set; }

        public required string ValidRangeTo { get; set; }

        public required string ParameterValue { get; set; }

        public required string Instructions { get; set; }
    }
}
