using CsvHelper.Configuration;

namespace EPR.Calculator.Frontend.Models
{
    public class ParameterTemplateValueMap : ClassMap<ParameterTemplateValue>
    {
        public ParameterTemplateValueMap()
        {
            Map(m => m.ParameterUniqueRef);
            Map(m => m.ParameterValue);
        }
    }
}
