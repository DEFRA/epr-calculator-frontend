using System.Diagnostics.CodeAnalysis;
using CsvHelper.Configuration;

namespace EPR.Calculator.Frontend.Models
{
    [ExcludeFromCodeCoverage]
    public class ParameterTemplateValueMap : ClassMap<ParameterTemplateValue>
    {
        public ParameterTemplateValueMap()
        {
            this.Map(m => m.ParameterUniqueRef);
            this.Map(m => m.ParameterValue);
        }
    }
}
