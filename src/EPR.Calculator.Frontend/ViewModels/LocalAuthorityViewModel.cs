using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Serialization;
using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels
{
    [ExcludeFromCodeCoverage]
    public class LocalAuthorityViewModel
    {
        public LocalAuthorityViewModel(LocalAuthorityDisposalCost localAuthorityDisposalCost)
        {
            Country = GetCountryDescription(localAuthorityDisposalCost.Country);
            Material = localAuthorityDisposalCost.Material;
            TotalCost = localAuthorityDisposalCost.TotalCost;
            CreatedBy = localAuthorityDisposalCost.CreatedBy;
            CreatedAt = GetFormattedCreatedAt(localAuthorityDisposalCost.CreatedAt);
            EffectiveFrom = localAuthorityDisposalCost.EffectiveFrom;
        }

        public string Country { get; set; }

        public string Material { get; set; }

        public decimal TotalCost { get; set; }

        public string CreatedBy { get; set; }

        public string CreatedAt { get; set; }

        public DateTime EffectiveFrom { get; set; }

        private string GetCountryDescription(string country)
        {
            return typeof(Country).GetTypeInfo().DeclaredMembers.SingleOrDefault(x => x.Name == country)?.GetCustomAttribute<EnumMemberAttribute>(false)?.Value;
        }

        private string GetFormattedCreatedAt(DateTime createdAt)
        {
            return createdAt.ToString("dd MMM yyyy ' at 'H:mm", new System.Globalization.CultureInfo("en-GB"));
        }
    }
}
