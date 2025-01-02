using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Serialization;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels
{
    /// <summary>
    /// View model to display the local authority disposal costs.
    /// </summary>

    [ExcludeFromCodeCoverage]
    public record LocalAuthorityViewModel : ViewModelCommonData
    {
        public required string LastUpdatedBy { get; init; }

        public required List<IGrouping<string, LocalAuthorityData>>? ByCountry { get; init; }

        private static string GetFormattedCreatedAt(DateTime createdAt)
        {
            return createdAt.ToString("dd MMM yyyy ' at 'H:mm", new System.Globalization.CultureInfo("en-GB"));
        }

        private static string GetTotalCost(decimal totalCost)
        {
            return totalCost == 0 ? string.Format("{0}{1:F0}", "£", totalCost) : string.Format("{0}{1:F2}", "£", totalCost);
        }

        public record LocalAuthorityData
        {
            public LocalAuthorityData(LocalAuthorityDisposalCost localAuthorityDisposalCost)
            {
                this.Country = localAuthorityDisposalCost.Country;
                this.Material = localAuthorityDisposalCost.Material;
                this.TotalCost = GetTotalCost(localAuthorityDisposalCost.TotalCost);
                this.CreatedBy = localAuthorityDisposalCost.CreatedBy;
                this.CreatedAt = GetFormattedCreatedAt(localAuthorityDisposalCost.CreatedAt);
                this.EffectiveFrom = localAuthorityDisposalCost.EffectiveFrom;
            }

            public string Country { get; set; }

            public string Material { get; set; }

            public string TotalCost { get; set; }

            public string CreatedBy { get; set; }

            public string CreatedAt { get; set; }

            public DateTime EffectiveFrom { get; set; }
        }
    }
}
