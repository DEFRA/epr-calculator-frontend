namespace EPR.Calculator.Frontend.Models
{
    public class CreateDefaultParameterSettingDto
    {
        public string ParameterYear { get; set; }
        public IEnumerable<SchemeParameterTemplateValue> SchemeParameterTemplateValues { get; set; }
    }
}
