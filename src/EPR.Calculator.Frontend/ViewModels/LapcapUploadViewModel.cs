using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels;

public record LapcapUploadViewModel
{
    public IReadOnlyList<CreateLapcapDataErrorDto>? LapcapErrors { get; set; }
    public IReadOnlyList<ValidationErrorDto>? ValidationErrors { get; set; }
    public IReadOnlyList<LapcapDataTemplateValueDto>? LapcapDataTemplateValue { get; set; }
    public IReadOnlyList<ErrorViewModel>? Errors { get; set; }
}
