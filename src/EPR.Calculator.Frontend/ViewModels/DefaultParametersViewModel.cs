using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels;

public record DefaultParametersViewModel
{
    public required string LastUpdatedBy { get; init; } = null!;
    public List<SchemeParametersViewModel> SchemeParameters { get; set; } = null!;
    public List<DefaultSchemeParametersLateReportingTonnage> LateReportingTonnageParams { get; set; } = null!;
    public DateTime LastUpdatedAt { get; set; }
    public bool IsDataAvailable { get; set; }
}
