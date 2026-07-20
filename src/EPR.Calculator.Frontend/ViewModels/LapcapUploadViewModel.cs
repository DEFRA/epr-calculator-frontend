using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels;

public record LapcapUploadViewModel
{
    public List<CreateLapcapDataErrorDto>? LapcapErrors { get; set; }
    public List<ValidationErrorDto>? ValidationErrors { get; set; }
    public List<LapcapDataTemplateValueDto>? LapcapDataTemplateValue { get; set; }
    public List<ErrorViewModel>? Errors { get; set; }
}
