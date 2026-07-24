using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels;

public record ParameterUploadViewModel
{
    public ErrorViewModel? Errors { get; set; }
    public IReadOnlyList<CreateDefaultParameterSettingErrorDto>? ParamterErrors { get; set; }
    public IReadOnlyList<ValidationErrorDto>? ValidationErrors { get; set; }
    public IReadOnlyList<SchemeParameterTemplateValue>? ParameterDataTemplateValue { get; set; }
}
