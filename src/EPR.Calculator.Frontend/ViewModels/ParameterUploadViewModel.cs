using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels;

public record ParameterUploadViewModel
{
    public ErrorViewModel? Errors { get; set; }
    public List<CreateDefaultParameterSettingErrorDto>? ParamterErrors { get; set; }
    public List<ValidationErrorDto>? ValidationErrors { get; set; }
    public List<SchemeParameterTemplateValue>? ParameterDataTemplateValue { get; set; }
    public string CsvTemplatePath => StaticHelpers.CsvTemplatePath;
    public string CsvTemplateFileName => StaticHelpers.CsvTemplateFileName;
}
