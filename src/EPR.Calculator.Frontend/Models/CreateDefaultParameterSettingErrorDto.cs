using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.Models
{
    [ExcludeFromCodeCoverage]
    public class CreateDefaultParameterSettingErrorDto
    {
        public string Message { get; set; }

        public string Description { get; set; }

        public string ParameterUniqueRef { get; set; }

        public string ParameterCategory { get; set; }

        public string ParameterType { get; set; }
    }
}
