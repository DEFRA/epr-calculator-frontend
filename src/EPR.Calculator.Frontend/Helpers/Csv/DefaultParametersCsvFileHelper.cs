using EPR.Calculator.Frontend.Mappers;
using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.Helpers.Csv;

public static class DefaultParametersCsvFileHelper
{
    public static Task<CsvParseResult<SetDefaultParametersRequest.ParameterValue>> Parse(
        IFormFile? fileUpload, CancellationToken cancellationToken)
    {
        return CsvFileHelper.ParseAsync<DefaultParametersCsvMapper, SetDefaultParametersRequest.ParameterValue>(
            fileUpload, cancellationToken: cancellationToken);
    }
}
