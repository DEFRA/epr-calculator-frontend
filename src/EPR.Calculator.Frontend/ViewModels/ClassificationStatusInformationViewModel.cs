namespace EPR.Calculator.Frontend.ViewModels;

public record ClassificationStatusInformationViewModel
{
    public bool ShowInitialRunDescription { get; set; }
    public string? InitialRunDescription { get; set; }
    public bool ShowInterimRecalculationRunDescription { get; set; }
    public string? InterimRecalculationRunDescription { get; set; }
    public bool ShowFinalRecalculationRunDescription { get; set; }
    public string? FinalRecalculationRunDescription { get; set; }
    public bool ShowFinalRunDescription { get; set; }
    public string? FinalRunDescription { get; set; }
}
