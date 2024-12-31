using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.ViewModels
{
    [ExcludeFromCodeCoverage]
    public record DefaultParametersViewModel : ViewModelCommonData
    {
        public required string LastUpdatedBy { get; init; }
    }
}
