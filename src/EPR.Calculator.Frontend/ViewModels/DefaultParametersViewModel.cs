namespace EPR.Calculator.Frontend.ViewModels
{
    public record DefaultParametersViewModel : ViewModelCommonData
    {
        public required string LastUpdatedBy { get; init; }
    }
}
