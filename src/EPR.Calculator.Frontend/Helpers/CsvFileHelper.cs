using CsvHelper;
using CsvHelper.Configuration;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace EPR.Calculator.Frontend.Helpers
{
    public static class CsvFileHelper
    {
        public static ErrorViewModel ValidateCSV(IFormFile fileUpload)
        {
            ErrorViewModel errorViewModel = new ErrorViewModel();

            if (fileUpload == null || fileUpload.Length == 0)
            {
                return errorViewModel = new ErrorViewModel() { DOMElementId = string.Empty, ErrorMessage = StaticHelpers.FileNotSelected };
            }

            if (!fileUpload.FileName.EndsWith(".csv"))
            {
                return errorViewModel = new ErrorViewModel() { DOMElementId = string.Empty, ErrorMessage = StaticHelpers.FileMustBeCSV };
            }

            if (fileUpload.Length > StaticHelpers.MaxFileSize)
            {
                return errorViewModel = new ErrorViewModel() { DOMElementId = string.Empty, ErrorMessage = StaticHelpers.FileNotExceed50KB };
            }

            return errorViewModel;
        }

        public static async Task<List<SchemeParameterTemplateValue>> PrepareDataForUpload(IFormFile fileUpload)
        {
            try
            {
                var schemeTemplateParameterValues = new List<SchemeParameterTemplateValue>();

                using var memoryStream = new MemoryStream(new byte[fileUpload.Length]);
                await fileUpload.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                using (var reader = new StreamReader(memoryStream))
                {
                    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        PrepareHeaderForMatch = header => Regex.Replace(header.ToString(), @"\s", string.Empty, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
                        ShouldSkipRecord = args => args.Row.Parser.Record.All(string.IsNullOrWhiteSpace) || args.Row.GetField(0).Contains("upload version")
                    };
                    using (var csv = new CsvReader(reader, config))
                    {
                        csv.Read();
                        while (csv.Read())
                        {
                            schemeTemplateParameterValues.Add(
                                new SchemeParameterTemplateValue() { ParameterUniqueReferenceId = csv.GetField(0), ParameterValue = csv.GetField(5) });
                        }
                    }
                }

                return schemeTemplateParameterValues;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
