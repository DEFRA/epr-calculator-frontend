using System.Globalization;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.Helpers
{
    public static class CsvFileHelper
    {
        public static ErrorViewModel ValidateCSV(IFormFile fileUpload)
        {
            ErrorViewModel errorViewModel = new ErrorViewModel();
            if (fileUpload == null)
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

        public static async Task<List<SchemeParameterTemplateValue>> PrepareSchemeParameterDataForUpload(IFormFile fileUpload)
        {
                var schemeTemplateParameterValues = new List<SchemeParameterTemplateValue>();

                using var memoryStream = new MemoryStream(new byte[fileUpload.Length]);
                await fileUpload.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                using (var reader = new StreamReader(memoryStream))
                {
                    var config = GetCsvConfiguration(UploadType.ParameterSettings);
                    using (var csv = new CsvReader(reader, config))
                    {
                    var csvReader = csv;
                    if (csvReader is not null)
                    {
                        csvReader.Read();
                        while (csvReader.Read())
                        {
                            var parameterUniqueReferenceId = csv?.GetField(0);
                            var parameterValue = csv?.GetField(5);
                            if (parameterUniqueReferenceId != null && parameterValue != null)
                            {
                                schemeTemplateParameterValues.Add(
                                       new SchemeParameterTemplateValue() { ParameterUniqueReferenceId = parameterUniqueReferenceId, ParameterValue = parameterValue });
                            }
                        }
                    }
                    }
                }

                return schemeTemplateParameterValues;
        }

        public static async Task<List<LapcapDataTemplateValueDto>> PrepareLapcapDataForUpload(IFormFile fileUpload)
        {
            var lapcapDataTemplateValues = new List<LapcapDataTemplateValueDto>();

            using var memoryStream = new MemoryStream(new byte[fileUpload.Length]);
            await fileUpload.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            using (var reader = new StreamReader(memoryStream))
            {
                var config = GetCsvConfiguration(UploadType.LapcapData);
                using (var csv = new CsvReader(reader, config))
                {
                    var csvReader = csv;
                    if (csvReader is not null)
                    {
                        csvReader.Read();
                        while (csvReader.Read())
                        {
                            var countryName = csv?.GetField(0);
                            var material = csv?.GetField(1);
                            var totalCost = csv?.GetField(2);

                            if (countryName != null && material != null && totalCost != null)
                            {
                                lapcapDataTemplateValues.Add(
                                    new LapcapDataTemplateValueDto() { CountryName = countryName, Material = material, TotalCost = totalCost });
                            }
                        }
                    }
                }
            }

            return lapcapDataTemplateValues;
        }

        private static CsvConfiguration GetCsvConfiguration(UploadType uploadType)
        {
            if (uploadType == UploadType.ParameterSettings)
            {
                return new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    PrepareHeaderForMatch = header => Regex.Replace(header.ToString(), @"\s", string.Empty, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
                    ShouldSkipRecord = args => args.Row.Parser.Record.All(string.IsNullOrWhiteSpace) || args.Row.GetField(0).Contains("upload version"),
                };
            }

            return new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = header => Regex.Replace(header.ToString(), @"\s", string.Empty, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
                ShouldSkipRecord = args => args.Row.Parser.Record.All(string.IsNullOrWhiteSpace),
            };
        }
    }
}
