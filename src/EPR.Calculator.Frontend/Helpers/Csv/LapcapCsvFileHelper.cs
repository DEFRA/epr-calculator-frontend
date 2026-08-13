using EPR.Calculator.Frontend.Mappers;
using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.Helpers.Csv;

public static class LapcapCsvFileHelper
{
    public static Task<CsvParseResult<CreateLapcapDataRequest.LapcapValue>> Parse(
        IFormFile? fileUpload, CancellationToken cancellationToken)
    {
        return CsvFileHelper.ParseAsync<LapcapCsvMapper, CreateLapcapDataRequest.LapcapValue>(
            fileUpload, cancellationToken: cancellationToken);
    }
}
