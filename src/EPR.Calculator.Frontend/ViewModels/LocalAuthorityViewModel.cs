using System.Globalization;
using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels;

public record LocalAuthorityViewModel : ViewModelCommonData
{
    public required string LastUpdatedBy { get; init; }
    public required List<IGrouping<string, LocalAuthorityData>>? ByCountry { get; init; }

    public record LocalAuthorityData
    {
        public LocalAuthorityData(LocalAuthorityDisposalCost localAuthorityDisposalCost)
        {
            Country = localAuthorityDisposalCost.Country;
            Material = localAuthorityDisposalCost.Material;
            TotalCost = GetTotalCost(localAuthorityDisposalCost.TotalCost);
            CreatedBy = localAuthorityDisposalCost.CreatedBy;
            CreatedAt = GetFormattedCreatedAt(localAuthorityDisposalCost.CreatedAt);
            EffectiveFrom = localAuthorityDisposalCost.EffectiveFrom;
        }

        public string Country { get; set; }

        public string Material { get; set; }

        public string TotalCost { get; set; }

        public string CreatedBy { get; set; }

        public string CreatedAt { get; set; }

        public DateTime EffectiveFrom { get; set; }

        private static string GetFormattedCreatedAt(DateTime createdAt)
        {
            return createdAt.ToString("dd MMM yyyy ' at 'H:mm", new CultureInfo("en-GB"));
        }

        private static string GetTotalCost(decimal totalCost)
        {
            var precision = totalCost == 0 ? 0 : 2;
            var culture = CultureInfo.CreateSpecificCulture("en-GB");
            culture.NumberFormat.CurrencySymbol = "£";
            culture.NumberFormat.CurrencyPositivePattern = 0;
            culture.NumberFormat.CurrencyGroupSeparator = ",";

            return totalCost.ToString($"C{precision}", culture);
        }
    }
}
