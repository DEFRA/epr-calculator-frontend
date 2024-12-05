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
    public class LocalAuthorityViewModel
    {
        public LocalAuthorityViewModel(LocalAuthorityDisposalCost localAuthorityDisposalCost)
        {
            this.Country = GetCountryDescription(localAuthorityDisposalCost.Country); this.Material = localAuthorityDisposalCost.Material;
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

        private static string GetCountryDescription(string country)
        {
            var countryDescription = typeof(Country).GetTypeInfo().DeclaredMembers.SingleOrDefault(x => x.Name == country)?.GetCustomAttribute<EnumMemberAttribute>(false)?.Value;
            if (countryDescription == null)
            {
                throw new ArgumentNullException(country, "Country is not returned by the local authority disposal costs API");
            }

            return countryDescription;
        }

        private static string GetFormattedCreatedAt(DateTime createdAt)
        {
            return createdAt.ToString("dd MMM yyyy ' at 'H:mm", new System.Globalization.CultureInfo("en-GB"));
        }

        private static string GetTotalCost(decimal totalCost)
        {
            return totalCost == 0 ? string.Format("{0}{1:F0}", "£", totalCost) : string.Format("{0}{1:F2}", "£", totalCost);
        }
    }
}
