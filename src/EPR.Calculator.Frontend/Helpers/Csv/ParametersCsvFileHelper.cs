using CsvHelper;
using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.Helpers.Csv;

public static class ParametersCsvFileHelper
{
    public static async Task<List<SchemeParameterTemplateValue>> Parse(IFormFile fileUpload)
    {
        var config = CsvFileHelper.DefaultCsvConfig;

        // Chains the default ShouldSkipRecord delegate
        var shouldSkip = config.ShouldSkipRecord!;
        config.ShouldSkipRecord = args =>
            shouldSkip.Invoke(args)
            || (args.Row.GetField(0) ?? "").Contains("upload version", StringComparison.OrdinalIgnoreCase);

        using var reader = new StreamReader(fileUpload.OpenReadStream());
        using var csvReader = new CsvReader(reader, config);
        await csvReader.ReadAsync();

        var schemeTemplateParameterValues = new List<SchemeParameterTemplateValue>();

        while (await csvReader.ReadAsync())
        {
            var parameterUniqueReferenceId = csvReader.GetField(0);
            var parameterValue = csvReader.GetField(5);
            if (parameterUniqueReferenceId != null && parameterValue != null)
            {
                schemeTemplateParameterValues.Add(
                    new SchemeParameterTemplateValue { ParameterUniqueReferenceId = parameterUniqueReferenceId, ParameterValue = parameterValue });
            }
        }

        return schemeTemplateParameterValues;
    }
}
