using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.Models
{
    [ExcludeFromCodeCoverage]
    public class ParameterTemplateValue
    {
        public string ParameterUniqueRef { get; set; }

        public string ParameterType { get; set; }

        public string ParameterCategory { get; set; }

        public string ValidRangeFrom { get; set; }

        public string ValidRangeTo { get; set; }

        public string ParameterValue { get; set; }

        public string Instructions { get; set; }
    }
}
