using EPR.Calculator.Frontend.Models;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.ViewModels
{
    [ExcludeFromCodeCoverage]
    public record DefaultParametersViewModel : ViewModelCommonData
    {
        public required string LastUpdatedBy { get; init; }

        public List<SchemeParametersViewModel> SchemeParameters { get; set; }

        public DateTime EffectiveFrom { get; set; }

        public bool IsDataAvailable { get; set; }
    }
}
