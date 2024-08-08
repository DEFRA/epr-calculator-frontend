using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.Models
{
    [ExcludeFromCodeCoverage]
    public class CreateDefaultParameterSettingDto
    {
        public string ParameterYear { get; set; }

        public IEnumerable<SchemeParameterTemplateValue> SchemeParameterTemplateValues { get; set; }
    }
}
