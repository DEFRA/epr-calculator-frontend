namespace EPR.Calculator.Frontend.ViewModels;

public record BackLinkViewModel
{
    public required string BackLink { get; set; }
    public required string CurrentUser { get; set; }
    public int? RunId { get; set; }
    public bool HideBackLink { get; set; }
}
