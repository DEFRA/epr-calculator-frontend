namespace EPR.Calculator.Frontend.ViewModels;

public record CalculatorRunOverviewViewModel : ViewModelCommonData
{
    public required CalculatorRunDetailsViewModel CalculatorRunDetails { get; set; }
}
