namespace EPR.Calculator.Frontend.ViewModels;

public record ViewModelCommonData
{
    public string CurrentUser { get; init; } = null!;
    public BackLinkViewModel? BackLinkViewModel { get; set; }
}
