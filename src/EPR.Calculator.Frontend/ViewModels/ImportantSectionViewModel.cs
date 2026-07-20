namespace EPR.Calculator.Frontend.ViewModels;

public record ImportantSectionViewModel
{
    public bool IsAnyRunInProgress { get; set; }
    public bool HasAnyDesigRun { get; set; }
    public int RunIdInProgress { get; set; }
    public bool IsDisplayInitialRun { get; set; }
    public string? IsDisplayInitialRunMessage { get; set; }
    public bool IsDisplayInterimRun { get; set; }
    public string? IsDisplayInterimRunMessage { get; set; }
    public bool IsDisplayFinalRecallRun { get; set; }
    public string? IsDisplayFinalRecallRunMessage { get; set; }
    public bool IsDisplayFinalRun { get; set; }
    public string? IsDisplayFinalRunMessage { get; set; }
    public bool IsDisplayTestRun { get; set; }
}
