using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels;

public record SchemeParametersViewModel
{
    public IEnumerable<DefaultSchemeParameters> DefaultSchemeParameters { get; set; } = null!;
    public string SchemeParameterName { get; set; } = null!;
    public bool IsDisplayPrefix { get; set; }
}
