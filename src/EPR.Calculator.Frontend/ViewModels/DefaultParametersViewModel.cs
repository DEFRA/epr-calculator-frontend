using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.ViewModels
{
    [ExcludeFromCodeCoverage]
    public record DefaultParametersViewModel : ViewModelCommonData
    {
        public required string LastUpdatedBy { get; init; } = null!;

        public List<SchemeParametersViewModel> SchemeParameters { get; set; } = null!;

        public DateTime EffectiveFrom { get; set; }

        public bool IsDataAvailable { get; set; }
    }
}
