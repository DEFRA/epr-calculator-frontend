using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels;

public record LapcapRefreshViewModel : ViewModelCommonData
{
    public IEnumerable<LapcapDataTemplateValueDto> LapcapTemplateValue { get; set; } = null!;
    public string FileName { get; set; } = null!;
}
