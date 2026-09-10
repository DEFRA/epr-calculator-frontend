using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.Helpers.Csv.Lapcap;

public static class LapcapCsvFileHelper
{
    public static Task<CsvParseResult<SetLapcapDataRequest.LapcapValue>> Parse(
        IFormFile? fileUpload, CancellationToken cancellationToken)
    {
        return CsvFileHelper.ParseAsync<LapcapCsvMapper, SetLapcapDataRequest.LapcapValue>(
            fileUpload, cancellationToken: cancellationToken);
    }
}
