using CsvHelper.Configuration;
using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.Mappers;

public sealed class DefaultParametersCsvMapper : ClassMap<SetDefaultParametersRequest.ParameterValue>
{
    private const string IdColumn = "Parameter Unique Ref";
    private const string ValueColumn = "Parameter Value";

    public DefaultParametersCsvMapper()
    {
        Map(m => m.Id).Name(IdColumn);
        Map(m => m.Value).Name(ValueColumn);
    }
}
