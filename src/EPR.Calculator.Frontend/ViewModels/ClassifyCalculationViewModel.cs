namespace EPR.Calculator.Frontend.ViewModels
{
    public record ClassifyCalculationViewModel(string CalcName,string FinancialYear, int CalculationId, bool? ClassificationType, string? RunDate) : ViewModelCommonData;
}